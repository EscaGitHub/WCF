using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;

namespace Swr.Capital1C.Service.Domain.Services.Boms
{
    public interface IBomCatalogService
    {
        Task<Bom> GetBomAsync(string id);

        Task UpdateBomAsync(Bom bom);
    }
}