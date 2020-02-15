using System;
using System.Collections.Generic;
using System.Linq;

namespace Swr.Capital1C.Service.Settings.Model
{
    public sealed class NomenclatureDefinition : CatalogDefinition
    {
        public NomenclatureDefinition()
        {
            FolderDefinitions = new List<FolderDefinition>();
            VariableMaps = new List<VariableMap>();
        }

        public override string StatusVariableValue { get; set; }
        public override string StatusVariableName { get; set; }
        public override List<FolderDefinition> FolderDefinitions { get; set; }

        public override List<VariableMap> VariableMaps { get; set; }
    }
}