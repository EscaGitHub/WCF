using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;

namespace Swr.Capital1C.Service.Infrastructure.Nomenclatures
{
    public interface ICatalogItemQuery
    {
        Task<CatalogItemId> FindByArticleAsync(string vendorCode);
    }
}
