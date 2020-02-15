using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Boms;
using Swr.Capital1C.Service.Domain.Services.Boms.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Infrastructure.Boms;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Xunit;

namespace IntegrationTest
{
    public class BomServiceTest
    {
        private const string Skip = "Run service";

        [Fact(Skip = "")]
        public async void UpdateStatusAsync_ShouldUpdateBomStatus()
        {
            var settings = DefaultSettingsForTests.Get();

            var catalogService = new BomCatalogService(settings.BomCatalogServiceConnection);

            var nomenclatureQuery = new CatalogItemQuery(settings.PdmDbConnection.ConnectionString(), "_Артикул_для_1С");
            var service = new BomService(catalogService, nomenclatureQuery);

            await service.UpdateStatusAsync("P0000037183", new ExportState
            {
                Status = "Готов",
                Message = "Всегда готов!"
            });
        }

        [Fact(Skip = "")]
        public async void UpdateStatusAsync_ShouldThrowBomNotFoundException()
        {
            var settings = DefaultSettingsForTests.Get();

            var catalogService = new BomCatalogService(settings.BomCatalogServiceConnection);

            var nomenclatureQuery = new CatalogItemQuery(settings.PdmDbConnection.ConnectionString(), "_Артикул_для_1С");
            var service = new BomService(catalogService, nomenclatureQuery);

            await Assert.ThrowsAsync<BomNotFoundException>(() => service.UpdateStatusAsync("P0100037179", new ExportState
            {
                Status = "Готов",
                Message = "Всегда готов!"
            }));
        }

    }
}
