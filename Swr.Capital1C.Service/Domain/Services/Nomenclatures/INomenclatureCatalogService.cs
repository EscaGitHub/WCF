using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures
{
    public interface INomenclatureCatalogService
    {
        Task<Nomenclature> GetNomenclatureAsync(string id);

        Task UpdateNomenclatureAsync(Nomenclature nomenclature);
    }
}