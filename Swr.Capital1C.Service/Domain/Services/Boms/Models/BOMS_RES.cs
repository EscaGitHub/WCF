using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models;
// ReSharper disable InconsistentNaming

namespace Swr.Capital1C.Service.Domain.Services.Boms.Models
{
    public class BOMS_RES
    {
        public HEADER HEADER { get; set; }

        public BOM[] BOMS { get; set; }
    }
}
