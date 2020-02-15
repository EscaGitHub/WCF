using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Okei;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Logger;
using Swr.Capital1C.Service.Settings;
using Swr.Capital1C.Service.Settings.Model;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures
{
    public class ItemsFactory
    {
        private readonly NLog.Logger _logger = LogHelper.Instance.GetLogger("Преобразование номенклатур");

        private readonly ICommonSettings _commonSettings;
        private readonly IOkeiService _okeiService;
        private readonly List<string> _errors;

        public List<string> Errors => _errors;

        public ItemsFactory(ICommonSettings commonSettings, IOkeiService okeiService)
        {
            _commonSettings = commonSettings;
            _okeiService = okeiService;
            _errors = new List<string>();
        }

        public ITEM[] Create(ref List<Nomenclature> nomenclatures)
        {
            _errors.Clear();

            _logger.Debug($"Пришло номенклатур для преобразования '{nomenclatures.Count}'");

            var settings = _commonSettings.NomenclatureDefinition;

            var result = new List<ITEM>();
            var correctNomenclature = new List<Nomenclature>();

            foreach (var nomenclature in nomenclatures)
            {
                try
                {
                    var item = new ITEM
                    {
                        ID = nomenclature.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("ID")),
                        NAME = GetNotation(nomenclature),
                        UOM = GetOkeiCode(nomenclature.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("UOM"))),
                        BOM_PART = nomenclature.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("BOM_PART")),
                        TYPE = GetView(nomenclature),
                        IS_PRODUCT = nomenclature.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("IS_PRODUCT")),
                        PURCHASED = nomenclature.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("PURCHASED")),
                        MATERIAL = nomenclature.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("MATERIAL")),
                        DENSITY = nomenclature.GetFloatOrNullValue(settings.GetAttributeNameByMessageAttribute("DENSITY")),
                        WEIGHT = nomenclature.GetFloatOrNullValue(settings.GetAttributeNameByMessageAttribute("WEIGHT")),
                        AREA = nomenclature.GetFloatOrNullValue(settings.GetAttributeNameByMessageAttribute("AREA")),
                        BEND = nomenclature.GetIntOrNullValue(settings.GetAttributeNameByMessageAttribute("BEND")),
                        LENGTH = nomenclature.GetFloatOrNullValue(settings.GetAttributeNameByMessageAttribute("LENGTH")),
                        WIDTH = nomenclature.GetFloatOrNullValue(settings.GetAttributeNameByMessageAttribute("WIDTH")),
                        THICK = nomenclature.GetFloatOrNullValue(settings.GetAttributeNameByMessageAttribute("THICK")),
                        LENGTH_CUT = nomenclature.GetFloatOrNullValue(settings.GetAttributeNameByMessageAttribute("LENGTH_CUT"))
                    };

                    if (string.IsNullOrWhiteSpace(item.ID))
                    {
                        var message = $"Не удалось получить Артикул номенклатуры с идентификатором '{nomenclature.Id}'. Номенклатура будет пропущена.";

                        _errors.Add(message);

                        _logger.Warn(message);

                        continue;
                    }

                    result.Add(item);
                    correctNomenclature.Add(nomenclature);
                }
                catch (OkeiCodeNotFoundException e)
                {
                    var article = nomenclature.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("ID"));

                    var message = $"Ошибка запроса кода ОКЕИ для номенклатуры с артикулом '{article}'";

                    _errors.Add(message + Environment.NewLine + e.Message);

                    _logger.Error(e, message);
                }
                catch (Exception e)
                {
                    var article = nomenclature.GetValueOrDefault(settings.GetAttributeNameByMessageAttribute("ID"));

                    var message = $"Ошибка обработки номенклатуры с артикулом '{article}'";

                    _errors.Add(message + Environment.NewLine + e.Message);

                    _logger.Error(e, message);
                }
            }

            _logger.Debug($"Преобразовано номенклатур для передачи '{result.Count}'");

            nomenclatures = correctNomenclature; // собираем оставшиеся для запипси в эксппортированные

            return result.ToArray();
        }

        private string GetView(Nomenclature nomenclature)
        {
            var view = "21 Полуфабрикат собствен";

            if (string.Equals("Материалы", nomenclature.GetValueOrDefault("Раздел спецификации"), StringComparison.OrdinalIgnoreCase))
            {
                view = "10-01 Сырье и материалы";
            }
            else if (string.Equals("Покупное", nomenclature.GetValueOrDefault("Вид воспроизводства"), StringComparison.OrdinalIgnoreCase))
            {
                view = "10-01 Сырье и материалы";
            }
            else if (string.Equals("1", nomenclature.GetValueOrDefault("Признак готовой продукции"), StringComparison.OrdinalIgnoreCase))
            {
                view = "43 Готовая продукция";
            }

            return view;
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

        private string GetNotation(Nomenclature nomenclature)
        {
            var notation = nomenclature.GetValueOrDefault("Наименование");

            var section = nomenclature.GetValueOrDefault("Раздел спецификации");

            if (string.Equals(section, "Детали", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(section, "Сборочные единицы", StringComparison.OrdinalIgnoreCase))
            {
                notation += " " + nomenclature.GetValueOrDefault("Обозначение");
            }

            return notation.Trim();
        }
    }
}
