using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swr.Capital1C.Service.Logger
{
    public  interface IDocumentContext
    {
        string FileId { get; set; }

        string FolderPath { get; set; }

        string Name { get; set; }

        string FolderId { get; set; }

        string Redaction { get; set; }

        string Configuration { get; set; }
    }
}
