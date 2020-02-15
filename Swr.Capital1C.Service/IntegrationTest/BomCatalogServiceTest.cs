using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Boms.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Infrastructure.Boms;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Xunit;

namespace IntegrationTest
{
    public class BomCatalogServiceTest
    {
        private const string Skip = "Run BOM service!";

        [Fact(Skip = "")]
        public async void SetToken_GetNomenclatureAsync_ShouldReturnNomenclature()
        {
            var nomenclatureService = new BomCatalogService(DefaultSettingsForTests.Get().BomCatalogServiceConnection);

            var result = await nomenclatureService.GetBomAsync("32580:По умолчанию:4");

            Assert.Equal("P0000037139", result.Attributes.First(t => t.Name == "Артикул родителя").Value);
            Assert.Equal(null, result.Attributes.First(t => t.Name == "Служебная запись").Value);
            Assert.Equal("Ручка", result.Attributes.First(t => t.Name == "Наименование").Value);
            Assert.Equal("A", result.Attributes.First(t => t.Name == "Версия").Value);
            Assert.Equal("1", result.Attributes.First(t => t.Name == "Кол-во выходного изделия").Value);

            Assert.Equal(3, result.Components.Length);

            var component = result.Components.First(t => t.Id == "32578:По умолчанию:2");
            Assert.Equal(3, component.Count);
            Assert.Equal("P0000037143", component.Attributes.First(t => t.Name == "Артикул").Value);
            Assert.Equal("шт", component.Attributes.First(t => t.Name == "Единица измерения количества").Value);
            Assert.Null(component.Attributes.First(t => t.Name == "Служебная запись").Value);
        }


        [Fact(Skip = "")]
        public async void SetToken_GetNomenclatureAsync_ShouldReturnBadRequest()
        {
            using (var bomService = new BomCatalogService(DefaultSettingsForTests.Get().BomCatalogServiceConnection))
            {
                await Assert.ThrowsAsync<BomIsNotSatisfyDefinitionException>(() => bomService.GetBomAsync("32580:@:4"));
            }
        }
    }
}
