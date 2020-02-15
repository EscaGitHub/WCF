using System;

namespace Swr.Capital1C.Service.Repositories
{
    public class ExportedItem
    {
        public ExportedItem(string id, DateTime modifiedDate, int documentType)
        {
            Id = id;
            ModifiedDate = modifiedDate;
            DocumentType = documentType;
        }

        public string Id { get; }

        public DateTime ModifiedDate { get; set; }

        public int DocumentType { get; }
    }
}