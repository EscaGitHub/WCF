using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Boms.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;

namespace Swr.Capital1C.Service.Domain.Services.Boms
{
    public class BomService
    {
        private readonly IBomCatalogService _catalogService;
        private readonly ICatalogItemQuery _bomQuery;

        public BomService(IBomCatalogService catalogService,
            ICatalogItemQuery bomQuery)
        {
            _catalogService = catalogService;
            _bomQuery = bomQuery;
        }

        public async Task UpdateStatusAsync(string vendorCode, ExportState state)
        {
            var bomId = await _bomQuery.FindByArticleAsync(vendorCode);

            if (bomId == null)
                throw new BomNotFoundException($"Спецификация не найдена по артикулу '{vendorCode}'");

            var bom = CreateBom(bomId, state);

            await _catalogService.UpdateBomAsync(bom);
        }

        private static Bom CreateBom(CatalogItemId bomId, ExportState state)
        {
            return new Bom
            {
                Id = bomId.ToString(),
                Attributes = new[]
                {
                    new Attribute
                    {
                        Name = "Статус передачи спецификации",
                        Value = state.Status
                    },
                    new Attribute
                    {
                        Name = "Описание ошибки спецификации",
                        Value = state.Message
                    }
                }
            };
        }
    }
}
