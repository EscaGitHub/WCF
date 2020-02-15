using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Okei;
using Swr.Capital1C.Service.Domain.Services.Boms.Models;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Logger;
using Swr.Capital1C.Service.Settings.Model;

namespace Swr.Capital1C.Service.Domain.Services.Boms
{
    public class BomsFactory
    {
        private readonly NLog.Logger _logger = LogHelper.Instance.GetLogger("Преобразование спецификаций");

        private readonly ICommonSettings _commonSettings;
        private readonly IOkeiService _okeiService;
        private readonly List<string> _errors;

        public List<string> Errors => _errors;

        public BomsFactory(ICommonSettings commonSettings, IOkeiService okeiService)
        {
            _commonSettings = commonSettings;
            _okeiService = okeiService;
            _errors = new List<string>();
        }

        public BOM[] Create(ref List<Bom> boms)
        {
            _errors.Clear();

            _logger.Debug($"Пришло спецификаций для преобразования '{boms.Count}'");

            var settings = _commonSettings.BomDefinition;

            var result = new List<BOM>();
            var correctBoms = new List<Bom>();
            
            foreach (var currentBom in boms)
            {
                try
                {
                    var bom = new BOM
                    {
                        PARENT_ID = currentBom.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("PARENT_ID")),
                        VERSION = currentBom.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("VERSION")),
                        BOM_NAME = currentBom.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("BOM_NAME")),
                        QTY_BOM = currentBom.GetFloatOrNullValue(settings.GetAttributeNameByMessageAttribute("QTY_BOM")),
                    };

                    if (string.IsNullOrWhiteSpace(bom.PARENT_ID))
                    {
                        var message = $"Не удалось получить Артикул родителя спецификации с идентификатором '{currentBom.Id}'. Спецификация будет пропущена.";

                        _errors.Add(message);

                        _logger.Warn(message);

                        continue;
                    }

                    var rows = new List<ROW>();

                    foreach (var currentBomComponent in currentBom.Components)
                    {
                        try
                        {
                            var row = new ROW
                            {
                                CHILD_ID = currentBomComponent.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("CHILD_ID")),
                                QTY = (float?)currentBomComponent.Count,
                                UOM = GetOkeiCode(currentBomComponent.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("UOM")))
                            };

                            if (string.IsNullOrWhiteSpace(row.CHILD_ID))
                            {
                                var message = $"Не удалось получить Артикул дочерней номенклатуры спецификации с идентификатором '{currentBom.Id}'. Спецификация будет пропущена.";

                                throw new ArgumentException(message);
                            }

                            rows.Add(row);
                        }
                        catch (OkeiCodeNotFoundException e)
                        {
                            var article = currentBomComponent.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("CHILD_ID"));

                            var message = $"Ошибка запроса кода ОКЕИ для дочерней номенклатуры с артикулом '{article}'";

                            _errors.Add(message + Environment.NewLine + e.Message);

                            _logger.Error(e, message);

                            throw; // выброс всей СП
                        }
                        catch (Exception e)
                        {
                            var article = currentBomComponent.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("CHILD_ID"));

                            var message = $"Ошибка обработки дочерней номенклатуры с артикулом '{article}'";

                            _errors.Add(message + Environment.NewLine + e.Message);

                            _logger.Error(e, message);

                            throw; // выброс всей СП
                        }
                    }

                    bom.ROWS = rows.ToArray();

                    result.Add(bom);
                    correctBoms.Add(currentBom);
                }
                catch (Exception e)
                {
                    var article = currentBom.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("PARENT_ID"));

                    var message = $"Ошибка обработки спецификации с артикулом '{article}'";

                    _errors.Add(message + Environment.NewLine + e.Message);

                    _logger.Error(e, message);
                }
            }

            _logger.Debug($"Преобразовано спецификаций для передачи '{result.Count}'");

            boms = correctBoms;

            return result.ToArray();
        }

        private string GetOkeiCode(string unitOfMeasure)
        {
            if (string.IsNullOrWhiteSpace(unitOfMeasure)) return string.Empty;

            _logger.Info($"Запрос кода ОКЕИ по обозначению '{unitOfMeasure}'...");

            var code = _okeiService.GetOkeiCodeAsync(unitOfMeasure).Result;

            _logger.Info($"Запрос кода ОКЕИ по обозначению '{unitOfMeasure}' выполнен.");

            if (code == null)
                throw new OkeiCodeNotFoundException(unitOfMeasure);

            return code;
        }
    }
}

