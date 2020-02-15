// ReSharper disable  InconsistentNaming

namespace Swr.Capital1C.Service.Domain.Services.Boms.Models
{
    public class BOM
    {
        public string PARENT_ID { get; set; }

        public string VERSION { get; set; }

        public string BOM_NAME { get; set; }

        public float? QTY_BOM { get; set; }

        public ROW[] ROWS { get; set; }
    }
}