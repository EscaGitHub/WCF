using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Properties;

namespace Swr.Capital1C.Service.Infrastructure.Nomenclatures
{
    public static class NomenclatureExtension
    {
        public static string GetValueOrDefault(this Nomenclature nomenclature, string name)
        {
            return nomenclature.Attributes.FirstOrDefault(t => t.Name == name)?.Value?.ToString();
        }

        public static bool GetBoolValue(this Nomenclature nomenclature, string name)
        {
            var value = nomenclature.GetValueOrDefault(name);

            return string.Equals("1", value, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals("yes", value, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals("да", value, StringComparison.OrdinalIgnoreCase);
        }

        public static float? GetFloatOrNullValue(this Nomenclature nomenclature, string name)
        {
            var value = nomenclature.GetValueOrDefault(name);

            if (string.IsNullOrWhiteSpace(value)) return null;

            value = value.Replace(",", ".");

            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        public static int? GetIntOrNullValue(this Nomenclature nomenclature, string name)
        {
            var value = nomenclature.GetValueOrDefault(name);

            if (string.IsNullOrWhiteSpace(value)) return null;

            return int.Parse(value);
        }
    }
}
