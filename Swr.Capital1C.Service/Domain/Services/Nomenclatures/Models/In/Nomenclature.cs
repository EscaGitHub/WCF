using System;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Properties;
using Attribute = Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In.Attribute;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In
{
    public class Nomenclature
    {
        public Nomenclature()
        {
            Attributes = new Attribute[0];
        }

        public string Id { get; set; }

        public DateTime ModifiedDate { get; set; }

        public Attribute[] Attributes { get; set; }

        public bool IsService(string attributeName)
        {
            return this.GetBoolValue(attributeName);
        }

        public bool IsAnnulled()
        {
            return this.GetBoolValue(Resources.IsAnnulled);
        }

        public bool IsServiceConfiguration()
        {
            return this.GetBoolValue(Resources.VersionNumberColumn);
        }
    }
}
