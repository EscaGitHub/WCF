using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest
{
    public class NomenclatureServiceTest
    {
        private const string Skip = "Run service";

        [Fact(Skip = "")]
        public async void UpdateStatusAsync_ShouldUpdateNomenclatureStatus()
        {
            var settings = DefaultSettingsForTests.Get();

            var catalogService = new NomenclatureCatalogService(settings.NomenclatureCatalogServiceConnection);

            var nomenclatureQuery = new CatalogItemQuery(settings.PdmDbConnection.ConnectionString(), "_Артикул_для_1С");
            var service = new NomenclatureService(catalogService, nomenclatureQuery);


            await service.UpdateStatusAsync("P0000036686", new ExportState
            {
                Status = "Готов? 3",
                Message = "Всегда готов! 3"
            });
        }
    }
}
