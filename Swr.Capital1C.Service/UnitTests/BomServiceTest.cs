using Moq;
using Swr.Capital1C.Service.Domain.Services.Boms;
using Swr.Capital1C.Service.Domain.Services.Boms.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Xunit;

namespace UnitTests
{
    public class BomServiceTest
    {
        private readonly Mock<IBomCatalogService> _catalogServiceMock;
        private readonly Mock<ICatalogItemQuery> _nomenclatureQueryMock;
        private Bom _bom;

        public BomServiceTest()
        {
            _catalogServiceMock = new Mock<IBomCatalogService>();
            _nomenclatureQueryMock = new Mock<ICatalogItemQuery>();
        }

        [Fact]
        public async void BomExists_UpdateStatus_ShouldUpdateBomExportStatusByArticle()
        {
            _nomenclatureQueryMock.Setup(t => t.FindByArticleAsync("P001"))
                .ReturnsAsync(() => new CatalogItemId(1, "-01", 3));

            _catalogServiceMock.Setup(t => t.UpdateBomAsync(It.IsAny<Bom>())).Callback((Bom n) => _bom = n);

            var service = new BomService(_catalogServiceMock.Object, _nomenclatureQueryMock.Object);

            const string vendorCode = "P001";
            var state = new ExportState
            {
                Status = "state",
                Message = "message"
            };

            await service.UpdateStatusAsync(vendorCode, state);

            _nomenclatureQueryMock.Verify(t => t.FindByArticleAsync("P001"));

            _catalogServiceMock.Verify(t => t.UpdateBomAsync(It.IsAny<Bom>()));

            Assert.Equal("1:-01:3", _bom.Id);

            Assert.Equal("Статус передачи спецификации", _bom.Attributes[0].Name);
            Assert.Equal("state", _bom.Attributes[0].Value);

            Assert.Equal("Описание ошибки спецификации", _bom.Attributes[1].Name);
            Assert.Equal("message", _bom.Attributes[1].Value);
        }

        [Fact]
        public async void BomNotExist_UpdateStatus_ShouldThrowException()
        {
            _nomenclatureQueryMock.Setup(t => t.FindByArticleAsync("P001"))
                .ReturnsAsync(() => null);

            var service = new BomService(_catalogServiceMock.Object, _nomenclatureQueryMock.Object);

            const string vendorCode = "P001";
            var state = new ExportState
            {
                Status = "state",
                Message = "message"
            };

            await Assert.ThrowsAsync<BomNotFoundException>(() => service.UpdateStatusAsync(vendorCode, state));

            _catalogServiceMock.Verify(t => t.UpdateBomAsync(It.IsAny<Bom>()), Times.Never);
        }
    }
}
