using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Boms.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Properties;
using Swr.Capital1C.Service.Settings.Model;
using Swr.Infrastructure.Validation.Domain.Models;
using Swr.Infrastructure.Validation.Extensions;

namespace Swr.Capital1C.Service.Domain.Services.Boms
{
    public class BomValidation : IBomValidation
    {
        public void Run(Bom bom)
        {
            var productState = GetProductState(bom);

            var componentStates = GetComponentStates(bom.Components).Where(t => t.HasErrors()).ToArray();

            if (productState.HasErrors()
                || componentStates.Any(t => t.HasErrors()))
            {
                var message = BuildMessage(bom, productState, componentStates);

                throw new BomIsInvalidException(message);
            }
        }

        private ModelState<Nomenclature> GetProductState(Nomenclature bom)
        {
            var modelState = new ModelState<Nomenclature>(bom);

            RuleFor("Артикул родителя", modelState).Required();
            RuleFor("Версия", modelState).Required();
            RuleFor("Наименование", modelState).Required();
            if(!Empty(modelState,"Кол-во выходного изделия")) RuleFor("Кол-во выходного изделия", modelState).Float();

            return modelState;
        }

        private bool Empty(ModelState<Nomenclature> modelState, string attributeName)
        {
            return string.IsNullOrWhiteSpace(modelState.Model.GetValueOrDefault(attributeName));
        }

        private static IEnumerable<ModelState<Component>> GetComponentStates(IEnumerable<Component> components)
        {
            return components.Select(GetComponentState).ToArray();
        }

        private static ModelState<Component> GetComponentState(Component component)
        {
            var modelState = new ModelState<Component>(component);
            
            RuleFor("Артикул", modelState).Required();
            //RuleFor("Количество", modelState).Required().Float();
            RuleFor("Единица измерения количества", modelState).Required();

            return modelState;
        }

        private static ValidatingProperty<Nomenclature> RuleFor(string name, ModelState<Nomenclature> modelState)
        {
            return new ValidatingProperty<Nomenclature>(name, modelState.Model.GetValueOrDefault(name), modelState);
        }

        private static ValidatingProperty<Component> RuleFor(string name, ModelState<Component> modelState)
        {
            return new ValidatingProperty<Component>(name, modelState.Model.GetValueOrDefault(name), modelState);
        }

        private static string BuildMessage(Nomenclature bom,
            ModelState<Nomenclature> productState,
            ICollection<ModelState<Component>> componentStates) 
        {
            var builder = new StringBuilder();

            var code = bom.GetValueOrDefault("Артикул родителя");
            var version = bom.GetValueOrDefault("Версия");
            builder.AppendLine($"Спецификация с кодом '{code}' (идентификатор '{bom.Id}') не удовлетворяет требованиям проверки.");

            var productMessage = BuildMessage(productState.GetPropertyStates());

            if (!string.IsNullOrEmpty(productMessage))
                builder.AppendLine(productMessage);

            if (componentStates.Any())
                builder.Append(BuildMessage(componentStates));

            return builder.ToString();
        }

        private static string BuildMessage(IEnumerable<ModelState<Component>> componentStates)
        {
            var builder = new StringBuilder();

            foreach (var componentState in componentStates)
            {
                var component = componentState.Model;
                var code = component.GetValueOrDefault("Артикул");
                builder.AppendLine($"Компонент спецификации с кодом '{code}' (идентификатор '{component.Id}') не удовлетворяет требованиям проверки.");
                builder.AppendLine(BuildMessage(componentState.GetPropertyStates()));
            }

            return builder.ToString();
        }

        private static string BuildMessage(IEnumerable<PropertyState> propertyStates)
        {
            var builder = new StringBuilder();

            foreach (var propertyState in propertyStates)
            {
                builder.AppendLine($"Атрибут '{propertyState.PropertyName}':");
                builder.Append(string.Join(Environment.NewLine, propertyState.GetErrors()));
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
