using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swr.Capital1C.Service.Repositories
{
    public interface IExportedItemRepository
    {
        Task<ExportedItem> GetByIdAndTypeAsync(string id, int documentType);

        Task AddAsync(ExportedItem item);

        Task UpdateAsync(ExportedItem item);
    }
}
