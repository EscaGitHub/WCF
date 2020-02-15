using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Okei;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Infrastructure;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Logger;
using Swr.Capital1C.Service.Repositories;
using Swr.Capital1C.Service.Settings.Model;
using Swr.Infrastructure.Mailing;
using Attribute = Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In.Attribute;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures
{
    public class NomenclatureChain
    {
        public List<string> Errors => _errors;

        public NomenclatureChain()
        {
            _errors = new List<string>();
        }

        private readonly NLog.Logger _logger = LogHelper.Instance.GetLogger("Цепочка получения номенклатуры");
        private readonly List<string> _errors;

        public async Task<ITEM[]> GetAsync(ICommonSettings commonSettings)
        {
            _errors.Clear();

            _logger.Info("Загрузка изменившихся номенклатур со статусом переменной...");

            var repository = new ChangedDocumentsRepository(commonSettings);
            var changedDocuments = repository.GetDocuments(CatalogTypeEnum.Nomenclature);

            _logger.Info("Фильтрация измененных номенклатур...");

            changedDocuments = ChangedDocumentsFiltering.Filter(changedDocuments);

            _logger.Info("Получение тел номенклатур из сервиса...");

            var nomenclatures = await GetNomenclatures(changedDocuments, commonSettings.NomenclatureCatalogServiceConnection);

            _logger.Info("Проверка загруженных данных...");

            nomenclatures = ValidateNomenclatures(nomenclatures, commonSettings.NomenclatureDefinition.GetAttributeNameByMessageAttribute("ID"));

            _logger.Info("Преобразование в сообщение ответ...");

            var okeiService = new OkeiService(commonSettings.OkeiServiceConnection);
            var itemsFactory = new ItemsFactory(commonSettings, new CachedOkeiService(okeiService));
            var items = itemsFactory.Create(ref nomenclatures);

            if (itemsFactory.Errors.Count > 0) 
                _errors.Add(string.Join("\n", itemsFactory.Errors));

            _logger.Info("Обновление статуса передачи номенклатур в 'Ожидание подтверждения передачи в 1С:УПП'");

            nomenclatures = await UpdateNomenclatureStatus(nomenclatures, commonSettings.NomenclatureCatalogServiceConnection);

            // Выключено после совещания: 03.02.2020
            //_logger.Info("Сохранение номенклатуры в переданные...");

            //SaveExportedStatus(nomenclatures, commonSettings);

            return items;
        }

        private async Task<List<Nomenclature>> UpdateNomenclatureStatus(IReadOnlyCollection<Nomenclature> nomenclatures, CatalogServiceConnection settings)
        {
            var result = new List<Nomenclature>();

            _logger.Debug($"Пришло номенклатур на обновление статуса '{nomenclatures.Count}'");

            using (var service = new NomenclatureCatalogService(settings))
            {
                foreach (var nomenclature in nomenclatures)
                {
                    var id = nomenclature.Id;

                    try
                    {
                        _logger.Info($"Запрос на обновление статуса номенклатуры по идентификатору '{id}'...");

                        await service.UpdateNomenclatureAsync(CreateNomenclature(nomenclature));

                        result.Add(nomenclature);

                        _logger.Info($"Запрос на обновление статуса номенклатуры по идентификатору '{id}' выполнен.");
                    }
                    catch (HttpRequestException e)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        var message = $"Ошибка при обновлении статуса номенклатуры с идентификатором '{id}'";
                        _errors.Add(message);
                        _logger.Warn(e, message);
                    }
                }
            }

            _logger.Debug($"Номенклатур после обновления статуса '{nomenclatures.Count}'");

            return result;
        }

        private static Nomenclature CreateNomenclature(Nomenclature nomenclature)
        {
            return new Nomenclature
            {
                Id = nomenclature.Id,
                Attributes = new[]
                {
                    new Attribute
                    {
                        Name = "Статус передачи номенклатуры",
                        Value = "Ожидание подтверждения передачи в 1С:УПП"
                    },
                }
            };
        }

        private List<Nomenclature> ValidateNomenclatures(List<Nomenclature> nomenclatures, string codeAttributeName)
        {
            var result = new List<Nomenclature>();

            var nomenclatureValidation = new NomenclatureValidation();

            try
            {
                _logger.Debug($"Номенклатур до проверки полей атрибутов '{nomenclatures.Count}'");

                foreach (var nomenclature in nomenclatures)
                {
                    try
                    {
                        nomenclatureValidation.Run(nomenclature, codeAttributeName);

                        result.Add(nomenclature);
                    }
                    catch (NomenclatureIsInvalidException e)
                    {
                        _errors.Add(e.Message);
                        _logger.Warn(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Ошибка при проверке номенклатур");
                throw;
            }

            _logger.Debug($"Номенклатур после проверки полей атрибутов '{result.Count}'");

            return result;
        }

        private void SaveExportedStatus(List<Nomenclature> nomenclatures, ICommonSettings settings)
        {
            var connectionString = settings.ExportedDocumentsStatusDbConnection.ConnectionString();
            var idVariableName = settings.NomenclatureDefinition.GetAttributeNameByMessageAttribute("ID");

            IExportedItemRepository repository = new ExportedItemRepository(connectionString);

            try
            {
                const int documentTypeId = (int) CatalogTypeEnum.Nomenclature;

                foreach (var nomenclature in nomenclatures)
                {
                    var article = nomenclature.GetValueOrDefault(idVariableName);

                    try
                    {
                        var latestExported = repository.GetByIdAndTypeAsync(article, documentTypeId).Result;

                        if (latestExported != null && nomenclature.ModifiedDate < latestExported.ModifiedDate)
                            throw new NomenclatureIsOutdatedException();

                        if (latestExported == null)
                        {
                            var task = repository.AddAsync(new ExportedItem(article, nomenclature.ModifiedDate, documentTypeId));

                            task.Wait();
                        }
                        else
                        {
                            latestExported.ModifiedDate = nomenclature.ModifiedDate;

                            repository.UpdateAsync(latestExported);
                        }
                    }
                    catch (NomenclatureIsOutdatedException e)
                    {
                        var message = $"Ранее была передана новая версия номенклатуры с кодом '{article}' (идентификатор '{nomenclature.Id}').";
                        _errors.Add(message);
                        _logger.Error(e, message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Ошибка при сохранении номенклатур в переданные");
                throw;
            }
        }

        private async Task<List<Nomenclature>> GetNomenclatures(IEnumerable<ChangedDocument> changedDocuments, CatalogServiceConnection settings)
        {
            var result = new List<Nomenclature>();

            using (var service = new NomenclatureCatalogService(settings))
            {
                foreach (var changedDocument in changedDocuments)
                {
                    var id = changedDocument.GetUniqueId();

                    try
                    {
                        _logger.Info($"Запрос данных номенклатуры по идентификатору '{id}'...");

                        var nomenclature = await service.GetNomenclatureAsync(id);

                        if (nomenclature != null) result.Add(nomenclature);
                        else
                        {
                            throw new NomenclatureNotFoundException();
                        }

                        _logger.Info($"Запрос данных номенклатуры по идентификатору '{id}' выполнен.");
                    }
                    catch (NomenclatureNotFoundException e)
                    {
                        var message = $"Номенклатура с идентификатором '{id}' не найдена.";
                        _errors.Add(message);
                        _logger.Warn(e, message);
                    }
                    catch (NomenclatureIsInvalidException e)
                    {
                        var message = $"Номенклатура с идентификатором '{id}' не корректна. Номенклатура не будет передана.";
                        _errors.Add(message + Environment.NewLine + e.Message);
                        _logger.Warn(e, message);
                    }
                    catch (NomenclatureIsNotSatisfyDefinitionException e)
                    {
                        var message = $"Номенклатура с идентификатором '{id}' не удовлетворяет определению. Номенклатура не будет передана.";
                        _errors.Add(message);
                        _logger.Warn(e, message);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"Ошибка при получении тела номенклатуры с артикулом '{changedDocument.Article}' и идентификатором '{id}'");
                        throw;
                    }
                }
            }

            _logger.Debug($"Получено тел номенклатур из сервиса '{result.Count}'");

            return result;
        }
    }
}
