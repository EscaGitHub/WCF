using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;

namespace Swr.Capital1C.Service.Domain.Services.Boms.Models.In
{
    public class Component : Nomenclature
    {
        public double Count { get; set; }
    }
}
