
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Okei;
using Swr.Capital1C.Service.Domain.Services.Boms.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Boms.Models;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Infrastructure;
using Swr.Capital1C.Service.Infrastructure.Boms;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Logger;
using Swr.Capital1C.Service.Repositories;
using Swr.Capital1C.Service.Settings.Model;
using Swr.Infrastructure.Mailing;
using Attribute = System.Attribute;

namespace Swr.Capital1C.Service.Domain.Services.Boms
{
    public class BomChain
    {
        private readonly NLog.Logger _logger = LogHelper.Instance.GetLogger("Цепочка получения спецификаций");
        private List<string> _errors;

        public List<string> Errors => _errors;

        public BomChain()
        {
            _errors = new List<string>();
        }

        public async Task<BOM[]> GetAsync(ICommonSettings commonSettings)
        {
            _errors.Clear();

            _logger.Info("Загрузка изменившихся спецификаций со статусом переменной...");

            var repository = new ChangedDocumentsRepository(commonSettings);
            var changedDocuments = repository.GetDocuments(CatalogTypeEnum.Specification);

            _logger.Info("Фильтрация измененных спецификаций...");

            changedDocuments = ChangedDocumentsFiltering.Filter(changedDocuments);

            _logger.Info("Получение тел спецификаций из сервиса...");

            var catalogBoms = await GetBoms(changedDocuments, commonSettings.BomCatalogServiceConnection);

            _logger.Info("Проверка входящих в спецификации номенклатур на ранее переданные...");

            catalogBoms = ValidateBomComponents(catalogBoms, commonSettings);

            _logger.Info("Проверка загруженных данных...");

            catalogBoms = ValidateBoms(catalogBoms, commonSettings.BomDefinition);

            _logger.Info("Преобразование в сообщение ответ...");

            var bomsFactory = new BomsFactory(commonSettings, new CachedOkeiService(new OkeiService(commonSettings.OkeiServiceConnection)));
            var boms = bomsFactory.Create(ref catalogBoms);

            if (bomsFactory.Errors.Count > 0)
                _errors.Add(string.Join("\n", bomsFactory.Errors));

            _logger.Info("Обновление статуса передачи номенклатур в 'Ожидание подтверждения передачи в 1С:УПП'");

            catalogBoms = await UpdateBomsStatus(catalogBoms, commonSettings.BomCatalogServiceConnection);

            // Выключено после совещания: 03.02.2020
            //_logger.Info("Сохранение спецификаций в переданные...");

            //SaveExportedStatus(catalogBoms, commonSettings);

            return boms;
        }

        private async Task<List<Bom>> UpdateBomsStatus(List<Bom> catalogBoms, CatalogServiceConnection settings)
        {
            var result = new List<Bom>();

            _logger.Debug($"Пришло номенклатур на обновление статуса '{catalogBoms.Count}'");

            using (var service = new BomCatalogService(settings))
            {
                foreach (var bom in catalogBoms)
                {
                    var id = bom.Id;

                    try
                    {
                        _logger.Info($"Запрос на обновление статуса Спецификаций по идентификатору '{id}'...");

                        await service.UpdateBomAsync(CreateBom(bom));

                        result.Add(bom);

                        _logger.Info($"Запрос на обновление статуса Спецификаций по идентификатору '{id}' выполнен.");
                    }
                    catch (HttpRequestException e)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        var message = $"Ошибка при обновлении статуса Спецификаций с идентификатором '{id}'";
                        _errors.Add(message);
                        _logger.Warn(e, message);
                    }
                }
            }

            _logger.Debug($"Спецификаций после обновления статуса '{catalogBoms.Count}'");

            return result;
        }

        private static Bom CreateBom(Bom nomenclature)
        {
            return new Bom
            {
                Id = nomenclature.Id,
                Attributes = new[]
                {
                    new Nomenclatures.Models.In.Attribute
                    {
                        Name = "Статус передачи спецификации",
                        Value = "Ожидание подтверждения передачи в 1С:УПП"
                    },
                }
            };
        }

        private List<Bom> ValidateBomComponents(List<Bom> catalogBoms, ICommonSettings settings)
        {
            var serviceAttributeAlias = "Служебная запись";

            var connectionString = settings.ExportedDocumentsStatusDbConnection.ConnectionString();
            var idVariableName = settings.BomDefinition.GetAttributeNameByMessageAttribute("CHILD_ID");

            IExportedItemRepository repository = new ExportedItemRepository(connectionString);

            var result = new List<Bom>();

            try
            {
                _logger.Debug($"Спецификаций до проверки компонентов '{catalogBoms.Count}'");

                foreach (var catalogBom in catalogBoms)
                {
                    try
                    {
                        foreach (var catalogBomComponent in catalogBom.Components)
                        {
                            if(catalogBomComponent.IsService(serviceAttributeAlias))
                                throw new BomIsInvalidException($"Спецификация с идентификатором '{catalogBom.Id}' содержит служебные компоненты.");

                            // Выключено после совещания: 03.02.2020
                            //var article = catalogBomComponent.GetValueOrDefault(idVariableName);

                            //var latestExported = repository.GetByIdAndTypeAsync(article, 0).Result;

                            //if(latestExported == null)
                            //    throw new BomIsInvalidException($"Спецификация с идентификатором '{catalogBom.Id}' содержит в составе не переданную ранее номенклатуру с идентификатором '{catalogBomComponent.Id}'.");
                        }

                        result.Add(catalogBom);
                    }
                    catch (BomIsInvalidException e)
                    {
                        _errors.Add(e.Message);
                        _logger.Warn(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Ошибка при проверке компонентов спецификаций");
                throw;
            }

            _logger.Debug($"Спецификаций после проверки компонентов '{result.Count}'");

            return result;
        }

        private List<Bom> ValidateBoms(List<Bom> catalogBoms, BomDefinition settings)
        {
            var result = new List<Bom>();

            var bomValidation = new BomValidation();

            try
            {
                _logger.Debug($"Спецификаций до проверки полей атрибутов '{catalogBoms.Count}'");

                foreach (var catalogBom in catalogBoms)
                {
                    try
                    {
                        bomValidation.Run(catalogBom);

                        result.Add(catalogBom);
                    }
                    catch (BomIsInvalidException e)
                    {
                        _errors.Add(e.Message);
                        _logger.Warn(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Ошибка при проверке спецификаций");
                throw;
            }

            _logger.Debug($"Спецификаций после проверки полей атрибутов '{result.Count}'");

            return result;
        }

        private void SaveExportedStatus(List<Bom> boms, ICommonSettings settings)
        {
            var connectionString = settings.ExportedDocumentsStatusDbConnection.ConnectionString();
            var idVariableName = settings.BomDefinition.GetAttributeNameByMessageAttribute("PARENT_ID");

            IExportedItemRepository repository = new ExportedItemRepository(connectionString);

            try
            {
                const int documentTypeId = (int)CatalogTypeEnum.Specification;

                foreach (var bom in boms)
                {
                    var article = bom.GetValueOrDefault(idVariableName);

                    try
                    {
                        var latestExported = repository.GetByIdAndTypeAsync(article, documentTypeId).Result;

                        if (latestExported != null && bom.ModifiedDate < latestExported.ModifiedDate)
                            throw new NomenclatureIsOutdatedException();

                        if (latestExported == null)
                        {
                            var task = repository.AddAsync(new ExportedItem(article, bom.ModifiedDate, documentTypeId));

                            task.Wait();
                        }
                        else
                        {
                            latestExported.ModifiedDate = bom.ModifiedDate;

                            repository.UpdateAsync(latestExported);
                        }
                    }
                    catch (NomenclatureIsOutdatedException e)
                    {
                        var message = $"Ранее была передана новая версия спецификации с кодом '{article}' (идентификатор '{bom.Id}').";
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

        private async Task<List<Bom>> GetBoms(List<ChangedDocument> changedDocuments, CatalogServiceConnection settings)
        {
            var result = new List<Bom>();

            using (var service = new BomCatalogService(settings))
            {
                foreach (var changedDocument in changedDocuments)
                {
                    var id = changedDocument.GetUniqueId();

                    try
                    {
                        _logger.Info($"Запрос данных спецификации по идентификатору '{id}'...");

                        var bom = await service.GetBomAsync(id);

                        if (bom != null) result.Add(bom);
                        else
                        {
                            throw new BomNotFoundException();
                        }

                        _logger.Info($"Запрос данных спецификации по идентификатору '{id}' выполнен.");
                    }
                    catch (BomNotFoundException e)
                    {
                        var message = $"Спецификация с идентификатором '{id}' не найдена.";
                        _errors.Add(message);
                        _logger.Warn(e, message);
                    }
                    catch (BomIsInvalidException e)
                    {
                        var message = $"Спецификация с идентификатором '{id}' не корректна. Номенклатура не будет передана.";
                        _errors.Add(message + Environment.NewLine + e.Message);
                        _logger.Warn(e, message);
                    }
                    catch (BomIsNotSatisfyDefinitionException e)
                    {
                        var message = $"Спецификация с идентификатором '{id}' не удовлетворяет определению. Спецификация не будет передана.";
                        _errors.Add(message);
                        _logger.Warn(e, message);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"Ошибка при получении тела спецификации с артикулом '{changedDocument.Article}' и идентификатором '{id}'");
                        throw;
                    }
                }
            }

            _logger.Debug($"Получено тел номенклатур из сервиса '{result.Count}'");

            return result;
        }
    }
}
