using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures
{
    public class ChangedDocument
    {
        public ChangedDocument(int id, string configurationName, int version)
        {
            Id = id;
            ConfigurationName = configurationName;
            Version = version;
        }

        public int Id { get; set; }

        public string ConfigurationName { get; set; }

        public int Version { get; set; }

        public string Article { get; set; }

        public string IsService { get; set; }

        public string GetUniqueId()
        {
            return $"{Id}:{ConfigurationName}:{Version}";
        }
    }
}
