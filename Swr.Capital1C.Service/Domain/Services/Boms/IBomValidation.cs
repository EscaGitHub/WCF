using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;
using Swr.Capital1C.Service.Settings.Model;

namespace Swr.Capital1C.Service.Domain.Services.Boms
{
    public interface IBomValidation
    {
        void Run(Bom bom);
    }
}
