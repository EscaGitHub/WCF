using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Xunit;

namespace UnitTests
{
    public class NomenclatureServiceTest
    {
        private readonly Mock<INomenclatureCatalogService> _catalogServiceMock;
        private readonly Mock<ICatalogItemQuery> _nomenclatureQueryMock;
        private Nomenclature _nomenclature;

        public NomenclatureServiceTest()
        {
            _catalogServiceMock = new Mock<INomenclatureCatalogService>();
            _nomenclatureQueryMock = new Mock<ICatalogItemQuery>();
        }

        [Fact]
        public async void NomenclatureExists_UpdateStatus_ShouldUpdateNomenclatureExportStatusByArticle()
        {
            _nomenclatureQueryMock.Setup(t => t.FindByArticleAsync("P001"))
                .ReturnsAsync(() => new CatalogItemId(1, "-01", 3));

            _catalogServiceMock.Setup(t => t.UpdateNomenclatureAsync(It.IsAny<Nomenclature>())).Callback((Nomenclature n) => _nomenclature = n);

            var service = new NomenclatureService(_catalogServiceMock.Object, _nomenclatureQueryMock.Object);

            const string vendorCode = "P001";
            var state = new ExportState
            {
                Status = "state",
                Message = "message"
            };

            await service.UpdateStatusAsync(vendorCode, state);

            _nomenclatureQueryMock.Verify(t => t.FindByArticleAsync("P001"));

            _catalogServiceMock.Verify(t => t.UpdateNomenclatureAsync(It.IsAny<Nomenclature>()));

            Assert.Equal("1:-01:3", _nomenclature.Id);

            Assert.Equal("Статус передачи номенклатуры", _nomenclature.Attributes[0].Name);
            Assert.Equal("state", _nomenclature.Attributes[0].Value);

            Assert.Equal("Описание ошибки номенклатуры", _nomenclature.Attributes[1].Name);
            Assert.Equal("message", _nomenclature.Attributes[1].Value);
        }

        [Fact]
        public async void NomenclatureNotExist_UpdateStatus_ShouldThrowException()
        {
            _nomenclatureQueryMock.Setup(t => t.FindByArticleAsync("P001"))
                .ReturnsAsync(() => null);

            var service = new NomenclatureService(_catalogServiceMock.Object, _nomenclatureQueryMock.Object);

            const string vendorCode = "P001";
            var state = new ExportState
            {
                Status = "state",
                Message = "message"
            };

            await Assert.ThrowsAsync<NomenclatureNotFoundException>(() => service.UpdateStatusAsync(vendorCode, state));

            _catalogServiceMock.Verify(t => t.UpdateNomenclatureAsync(It.IsAny<Nomenclature>()), Times.Never);
        }
    }
}
