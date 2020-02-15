using System.Collections.Generic;

namespace Swr.Capital1C.Service.Settings.Model
{
    public class FolderDefinition
    {
        public List<string>  FolderPaths { get; set; } = new List<string>();

        public List<string> Extensions { get; set; } = new List<string>();

    }
}