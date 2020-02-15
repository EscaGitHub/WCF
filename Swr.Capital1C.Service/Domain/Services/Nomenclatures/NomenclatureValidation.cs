using System;
using System.Text;
using System.Web.UI.WebControls;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Properties;
using Swr.Capital1C.Service.Settings;
using Swr.Capital1C.Service.Settings.Model;
using Swr.Infrastructure.Validation.Domain.Models;
using Swr.Infrastructure.Validation.Extensions;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures
{
    public class NomenclatureValidation : INomenclatureValidation
    {
        private ModelState<Nomenclature> _modelState;

        public void Run(Nomenclature nomenclature, string codeAttributeName)
        {
            _modelState = new ModelState<Nomenclature>(nomenclature);
            RuleFor("Артикул").Required().MaxLength(11);
            RuleFor("Наименование").Required();
            RuleFor("Единица измерения").Required();
            RuleFor("Раздел спецификации").Required().InList(new[]
            {
                "Детали",
                "Комплексы",
                "Комплекты",
                "Материалы",
                "Прочие изделия",
                "Сборочные единицы",
                "Стандартные изделия"
            });
            //RuleFor("Вид номенклатуры").Required();
            if(!Empty("Признак готовой продукции"))
                RuleFor("Признак готовой продукции").InList(new []
                {
                    "1", "0"
                });
            RuleFor("Вид воспроизводства").Required().InList(new []
            {
                "Покупное",
                "Собственного изготовления",
                "Покупное с доработкой",
                "По кооперации"
            });
            RuleFor("Материал").MaxLength(11);
            if(!Empty("Плотность")) RuleFor("Плотность").Float();
            if (!Empty("Масса")) RuleFor("Масса").Float();
            if (!Empty("Площадь поверхности")) RuleFor("Площадь поверхности").Float();
            if (!Empty("Количество гибов")) RuleFor("Количество гибов").Float();
            if (!Empty("Общая длина")) RuleFor("Общая длина").Float();
            if (!Empty("Общая ширина")) RuleFor("Общая ширина").Float();
            if (!Empty("Длина реза")) RuleFor("Длина реза").Float();

            if (_modelState.HasErrors())
            {
                var message = BuildMessage(nomenclature, _modelState, codeAttributeName);
                throw new NomenclatureIsInvalidException(message);
            }
        }

        private bool Empty(string attributeName)
        {
            return string.IsNullOrWhiteSpace(_modelState.Model.GetValueOrDefault(attributeName));
        }

        private ValidatingProperty<Nomenclature> RuleFor(string name)
        {
            return new ValidatingProperty<Nomenclature>(name, _modelState.Model.GetValueOrDefault(name), _modelState);
        }

        private static string BuildMessage(Nomenclature nomenclature, ModelState<Nomenclature> modelState, string codeAttributeName)
        {
            var builder = new StringBuilder();

            var nomenclatureDefinition = CommonSettingsController.Settings.NomenclatureDefinition;

            var code = nomenclature.GetValueOrDefault(codeAttributeName);
            builder.AppendLine($"Номенклатура с артикулом '{code}' (идентификатор '{nomenclature.Id}') не удовлетворяет требованиям проверки.");

            foreach (var propertyState in modelState.GetPropertyStates())
            {
                builder.AppendLine($"Атрибут '{propertyState.PropertyName}':");

                builder.Append(string.Join(Environment.NewLine, propertyState.GetErrors()));

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}