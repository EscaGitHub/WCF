using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Settings.Model;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest
{
    public class NomenclatureCatalogServiceTest
    {
        private const string Skip = "Run service!";

        [Fact(Skip = "")]
        public async void SetToken_GetNomenclatureAsync_ShouldReturnNomenclature()
        {
            var nomenclatureService = new NomenclatureCatalogService(DefaultSettingsForTests.Get().NomenclatureCatalogServiceConnection);

            var result = await nomenclatureService.GetNomenclatureAsync("32422:По умолчанию:2");

            Assert.Equal("SWR.000.010", result.Attributes.First(t => t.Name == "Обозначение").Value);
            Assert.Equal("Скоба", result.Attributes.First(t => t.Name == "Наименование").Value);
        }

        [Fact(Skip = "")]
        public async void SetToken_GetNomenclatureAsync_ShouldReturnBadRequest()
        {
            using (var nomenclatureService = new NomenclatureCatalogService(DefaultSettingsForTests.Get().NomenclatureCatalogServiceConnection))
            {
                await Assert.ThrowsAsync<NomenclatureIsNotSatisfyDefinitionException>(() => nomenclatureService.GetNomenclatureAsync("32422:@:2"));
            }
        }
    }
}
