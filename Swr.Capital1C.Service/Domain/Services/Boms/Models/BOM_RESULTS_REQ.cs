using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models;
// ReSharper disable InconsistentNaming

namespace Swr.Capital1C.Service.Domain.Services.Boms.Models
{
    public class BOM_RESULTS_REQ
    {
        public HEADER HEADER { get; set; }

        public BOM_RESULT[] BOM_RESULTS { get; set; }
    }
}
