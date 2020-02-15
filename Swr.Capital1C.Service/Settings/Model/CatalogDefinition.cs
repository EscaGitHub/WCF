using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Swr.Capital1C.Service.Settings.Model
{
    [XmlInclude(typeof(NomenclatureDefinition))]
    [XmlInclude(typeof(BomDefinition))]
    public abstract class CatalogDefinition
    {
        public virtual string StatusVariableName { get; set; }
        public virtual string StatusVariableValue { get; set; }
        public virtual List<FolderDefinition> FolderDefinitions { get; set; }

        public virtual List<VariableMap> VariableMaps { get; set; }

        public string GetAttributeNameByMessageAttribute(string messageAttribute)
        {
            return VariableMaps.First(t => string.Equals(messageAttribute, t.MessageAttributeName, StringComparison.OrdinalIgnoreCase)).AttributeName;
        }

        public string GetMessageAttributeNameByAttribute(string attribute)
        {
            return VariableMaps.First(t => string.Equals(attribute, t.AttributeName, StringComparison.OrdinalIgnoreCase)).MessageAttributeName;
        }
    }
}