using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

// ReSharper disable InconsistentNaming

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models
{
    public class ITEM
    {
        public string ID { get; set; }

        public string NAME { get; set; }

        public string UOM { get; set; }

        public string BOM_PART { get; set; }

        public string TYPE { get; set; }

        public string IS_PRODUCT { get; set; }

        public string PURCHASED { get; set; }

        public string MATERIAL { get; set; }

        public float? WEIGHT { get; set; }

        public float? DENSITY { get; set; }

        public float? AREA { get; set; }

        public int? BEND { get; set; }

        public float? LENGTH { get; set; }

        public float? WIDTH { get; set; }

        public float? THICK { get; set; }

        public float? LENGTH_CUT { get; set; }
    }
}
