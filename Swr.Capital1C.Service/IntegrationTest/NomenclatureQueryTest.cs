using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Xunit;

namespace IntegrationTest
{
    public class NomenclatureQueryTest
    {
        private const string ConnectionString = "Data Source=192.168.1.13;Initial Catalog=PDM;User ID=sa;Password=qwe123PDM;";

        [Fact]
        public async void NomenclatureContainsVendorCode_BuildQuery_ShouldReturnNomenclature()
        {
            var query = new CatalogItemQuery(ConnectionString, "_Артикул_для_1С");

            var id = await query.FindByArticleAsync("P0000036651");

            Assert.Equal(32444, id.FileId);
            Assert.Equal(11, id.Version);
            Assert.Equal("По умолчанию", id.Configuration);
        }

        [Fact]
        public async void NoNomenclatureWithDesignationValue_BuildQuery_ShouldNothing()
        {
            var query = new CatalogItemQuery(ConnectionString, "Обозначение");

            var id = await query.FindByArticleAsync("1234567890абвгд");

            Assert.Null(id);
        }

        [Fact]
        public async void NomenclatureContainsDesignationInPreviousVersion_BuildQuery_ShouldReturnNothing()
        {
            var query = new CatalogItemQuery(ConnectionString, "Обозначение");

            var id = await query.FindByArticleAsync("SWR.006.002");

            Assert.Null(id);
        }

        [Fact]
        public async void NomenclatureIsDeleted_BuildQuery_ShouldReturnNothing()
        {
            var query = new CatalogItemQuery(ConnectionString, "Обозначение");

            var id = await query.FindByArticleAsync("SWR.006.003");

            Assert.Null(id);
        }

        [Fact]
        public async void TwoNomenclaturesWithSameDesignation_BuildQuery_ShouldThrowException()
        {
            var query = new CatalogItemQuery(ConnectionString, "Обозначение");

            await Assert.ThrowsAsync<FoundMoreThanOneCatalogItemsException>(() => query.FindByArticleAsync("SWR.006.005"));
        }
    }
}
