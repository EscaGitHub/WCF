using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Swr.Capital1C.Service.Domain.Services.Boms;
using Swr.Capital1C.Service.Domain.Services.Boms.Models;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models;
using Swr.Capital1C.Service.Infrastructure.Boms;
using Swr.Capital1C.Service.Infrastructure.Email;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Logger;
using Swr.Capital1C.Service.Settings;
using Swr.Capital1C.Service.Settings.Model;
using Swr.Infrastructure.Mailing;

namespace Swr.Capital1C.Service
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class Service : IService
    {
        public Service(ICommonSettings settings)
        {
            _settings = settings;
        }

        public Service()
        {
            _settings = CommonSettingsController.Settings;
        }

        private readonly NLog.Logger _logger = LogHelper.Instance.GetLogger("Служба интеграции");
        private readonly ICommonSettings _settings;

        public async Task<bool> IsAvailableAsync()
        {
            return await Task.Factory.StartNew(() => true);
        }

        #region Nomenclature

        public async Task<ITEMS_RES> GetNomenclaturesAsync(ITEMS_REQ request)
        {
            ITEMS_RES itemsResponse;

            using (new Session(request.HEADER.MSGID, Environment.MachineName, Environment.UserName))
            {
                _logger.TraceWithContext(request, "In: Get Nomenclatures");

                _logger.Info("Запрос на получение измененных номенклатур...");

                try
                {
                    var nomenclatureChain = new NomenclatureChain();

                    itemsResponse = new ITEMS_RES
                    {
                        HEADER = new HEADER { MSGID = request.HEADER.MSGID },

                        ITEMS = await nomenclatureChain.GetAsync(_settings)
                    };

                    if (nomenclatureChain.Errors.Count > 0)
                        await Task.Run(() => TrySendEmailAsync(nomenclatureChain.Errors,
                            "Ошибки экспорта номенклатуры",
                            "Ошибки в процессе экспорта номенклатуры из SWE-PDM в 1C:УПП",
                            _settings.EmailServiceSettings));

                    itemsResponse.HEADER.TIMESTAMP = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    _logger.Info($"Получено номенклатур для передачи '{itemsResponse.ITEMS.Length}'");

                    _logger.TraceWithContext(itemsResponse, "Out: Get Nomenclatures");
                }
                catch (Exception e)
                {
                    _logger.Error(e);

                    await Task.Run(() => TrySendEmailAsync(e,_settings.EmailServiceSettings));

                    throw;
                }
            }

            return itemsResponse;
        }

        public async Task<ITEM_RESULTS_RES> SetNomenclaturesResponseAsync(ITEM_RESULTS_REQ request)
        {
            ITEM_RESULTS_RES itemsResponse;

            using (new Session(request.HEADER.MSGID, Environment.MachineName, Environment.UserName))
            {
                _logger.TraceWithContext(request, "In: Set Nomenclatures Response");

                _logger.Info("Запрос с результатами добавления номенклатуры...");

                try
                {
                    itemsResponse = new ITEM_RESULTS_RES
                    {
                        HEADER = new HEADER { MSGID = request.HEADER.MSGID },
                    };

                    var catalogService = new NomenclatureCatalogService(_settings.NomenclatureCatalogServiceConnection);

                    var nomenclatureQuery = new CatalogItemQuery(_settings.PdmDbConnection.ConnectionString(), _settings.ArticleVariableName);
                    var service = new NomenclatureService(catalogService, nomenclatureQuery);

                    var errors = new List<string>();

                    foreach (var itemResult in request.ITEM_RESULTS)
                    {
                        _logger.Debug($"Обрабатывается результат для номенклатуры с атрибутом '{itemResult.ID}'");

                        var message = await UpdateNomenclatureStatusAsync(itemResult, service);

                        if (!string.IsNullOrEmpty(message))
                            errors.Add(message);
                    }

                    if (errors.Count > 0)
                        await Task.Run(() => TrySendEmailAsync(errors,
                            "Ошибки при сохранении результатов добавления номенклатуры",
                            "Ошибки в процессе сохранения результатов добавления номенклатуры из SWE-PDM в 1C:УПП",
                            _settings.EmailServiceSettings));

                    itemsResponse.HEADER.TIMESTAMP = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    _logger.Info("Запрос с результатами добавления номенклатуры окончен...");

                    _logger.TraceWithContext(itemsResponse, "Out: Set Nomenclatures Response");
                }
                catch (Exception e)
                {
                    _logger.Error(e);

                    await Task.Run(() => TrySendEmailAsync(e, _settings.EmailServiceSettings));

                    throw;
                }
            }

            return itemsResponse;
        }


        #endregion

        #region Specification

        public async Task<BOMS_RES> GetBomsAsync(BOMS_REQ request)
        {
            BOMS_RES itemsResponse;

            using (new Session(request.HEADER.MSGID, Environment.MachineName, Environment.UserName))
            {
                _logger.TraceWithContext(request, "In: Get Boms");

                _logger.Info("Запрос на получение измененных спецификаций...");

                try
                {
                    var bomChain = new BomChain();

                    itemsResponse = new BOMS_RES
                    {
                        HEADER = new HEADER { MSGID = request.HEADER.MSGID },

                        BOMS = await bomChain.GetAsync(_settings)
                    };

                    if (bomChain.Errors.Count > 0)
                        await Task.Run(() => TrySendEmailAsync(bomChain.Errors,
                            "Ошибки экспорта спецификаций",
                            "Ошибки в процессе экспорта спецификаций из SWE-PDM в 1C:УПП",
                           _settings.EmailServiceSettings));

                    itemsResponse.HEADER.TIMESTAMP = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    _logger.Info($"Получено спепцификаций для передачи '{itemsResponse.BOMS.Length}'");

                    _logger.TraceWithContext(itemsResponse, "Out: Get Boms");
                }
                catch (Exception e)
                {
                    _logger.Error(e);

                    await Task.Run(() => TrySendEmailAsync(e, _settings.EmailServiceSettings));

                    throw;
                }
            }

            return itemsResponse;
        }

        public async Task<BOM_RESULTS_RES> SetBomsResponseAsync(BOM_RESULTS_REQ request)
        {
            BOM_RESULTS_RES bomsResponse;

            using (new Session(request.HEADER.MSGID, Environment.MachineName, Environment.UserName))
            {
                _logger.TraceWithContext(request, "In: Set Boms Response");

                _logger.Info("Запрос с результатами добавления спецификаций...");

                try
                {
                    bomsResponse = new BOM_RESULTS_RES
                    {
                        HEADER = new HEADER { MSGID = request.HEADER.MSGID }
                    };

                    var catalogService = new BomCatalogService(_settings.BomCatalogServiceConnection);

                    var nomenclatureQuery = new CatalogItemQuery(_settings.PdmDbConnection.ConnectionString(), _settings.ArticleVariableName);
                    var service = new BomService(catalogService, nomenclatureQuery);

                    var errors = new List<string>();

                    foreach (var bomResult in request.BOM_RESULTS)
                    {
                        _logger.Debug($"Обрабатывается результат для спецификации с атрибутом '{bomResult.ID}'");

                        var message = await UpdateBomStatusAsync(bomResult, service);

                        if (!string.IsNullOrEmpty(message))
                            errors.Add(message);
                    }

                    if (errors.Count > 0)
                        await Task.Run(() => TrySendEmailAsync(errors,
                            "Ошибки при сохранении результатов добавления спецификаций",
                            "Ошибки в процессе сохранения результатов добавления спецификаций из SWE-PDM в 1C:УПП",
                            _settings.EmailServiceSettings));

                    bomsResponse.HEADER.TIMESTAMP = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    _logger.Info("Запрос с результатами добавления спецификаций окончен...");

                    _logger.TraceWithContext(bomsResponse, "Out: Set Boms Response");
                }
                catch (Exception e)
                {
                    _logger.Error(e);

                    await Task.Run(() => TrySendEmailAsync(e, _settings.EmailServiceSettings));

                    throw;
                }
            }

            return bomsResponse;
        }

        #endregion

        #region Email

        private async Task TrySendEmailAsync(Exception exception, EmailServiceSettings settings)
        {
            try
            {
                var emailService = new EmailService(settings);

                await emailService.SendAsync(EmailMessageFactory.SetUnexpectedError(exception));
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private async Task TrySendEmailAsync(IEnumerable<string> errors, string subject, string header, EmailServiceSettings settings)
        {
            try
            {
                var emailService = new EmailService(settings);

                await emailService.SendAsync(EmailMessageFactory.SetErrorMessages(errors, subject, header));
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        #endregion

        #region Support methods
        private async Task<string> UpdateNomenclatureStatusAsync(ITEM_RESULT itemResult, NomenclatureService service)
        {
            var message = string.Empty;

            try
            {
                await service.UpdateStatusAsync(itemResult.ID, new ExportState
                {
                    Status = itemResult.RESULT == 0 ? "Проверка не пройдена" : "Передана в 1С:УПП",
                    Message = itemResult.MESSAGE
                });
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception e)
            {
                message = $"Ошибка при обновлении номенклатуры с артикулом '{itemResult.ID}'";

                _logger.Error(e, message);

                message += Environment.NewLine + e.Message;
            }

            return message;
        }

        private async Task<string> UpdateBomStatusAsync(BOM_RESULT bomResult, BomService service)
        {
            var message = string.Empty;

            try
            {
                await service.UpdateStatusAsync(bomResult.ID, new ExportState
                {
                    Status = bomResult.RESULT == 0 ? "Проверка не пройдена" : "Передана в 1С:УПП",
                    Message = bomResult.MESSAGE
                });
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception e)
            {
                message = $"Ошибка при обновлении спецификации с артикулом '{bomResult.ID}'";

                _logger.Error(e, message);

                message += Environment.NewLine + e.Message;
            }

            return message;
        }

        #endregion
    }
}
