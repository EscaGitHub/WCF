using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures
{
    public class NomenclatureService
    {
        private readonly INomenclatureCatalogService _catalogService;
        private readonly ICatalogItemQuery _nomenclatureQuery;

        public NomenclatureService(INomenclatureCatalogService catalogService, 
            ICatalogItemQuery nomenclatureQuery)
        {
            _catalogService = catalogService;
            _nomenclatureQuery = nomenclatureQuery;
        }

        public async Task UpdateStatusAsync(string vendorCode, ExportState state)
        {
            var nomenclatureId = await _nomenclatureQuery.FindByArticleAsync(vendorCode);

            if (nomenclatureId == null)
                throw new NomenclatureNotFoundException($"Номенклатура не найдена по артикулу '{vendorCode}'");

            var nomenclature = CreateNomenclature(nomenclatureId, state);

            await _catalogService.UpdateNomenclatureAsync(nomenclature);
        }

        private static Nomenclature CreateNomenclature(CatalogItemId nomenclatureId, ExportState state)
        {
            return new Nomenclature
            {
                Id = nomenclatureId.ToString(),
                Attributes = new []
                {
                    new Attribute
                    {
                        Name = "Статус передачи номенклатуры",
                        Value = state.Status
                    },
                    new Attribute
                    {
                        Name = "Описание ошибки номенклатуры",
                        Value = state.Message
                    }
                }
            };
        }
    }
}
