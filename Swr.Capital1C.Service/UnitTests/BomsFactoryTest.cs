using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Swr.Capital1C.Okei;
using Swr.Capital1C.Service.Domain.Services.Boms;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Settings;
using Xunit;
using Attribute = Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In.Attribute;

namespace UnitTests
{
    public class BomsFactoryTest
    {
        [Fact]
        public void Standard_Create_ShouldReturnBom()
        {
            var bom = new Bom
            {
                Attributes = new []
                {
                    new Attribute{Name = "Артикул родителя", Value = "PB00001" },
                    new Attribute{Name = "Версия", Value = "2" },
                    new Attribute{Name = "Наименование", Value = "Наименование" },
                    new Attribute{Name = "Кол-во выходного изделия", Value = "2.3" },
                },
                Components = new []
                {
                    new Component
                    {
                        Count = 12.3,
                        Attributes = new []
                        {
                            new Attribute{Name = "Артикул", Value = "PB00002" },
                            new Attribute{Name = "Единица измерения количества", Value = "шт." },
                        }
                    } 
                }
            };

            var okeiServiceMock = new Mock<IOkeiService>();
            okeiServiceMock.Setup(t => t.GetOkeiCodeAsync(It.IsAny<string>())).ReturnsAsync(() => "123");

            var bomFactory = new BomsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);

            var boms = new List<Bom> { bom };

            var item = bomFactory.Create(ref boms)[0];

            Assert.Equal("PB00001", item.PARENT_ID);
            Assert.Equal("2", item.VERSION);
            Assert.Equal("Наименование", item.BOM_NAME);
            Assert.Equal(2.3f, item.QTY_BOM);

            Assert.Single(item.ROWS);
            Assert.Single(boms);

            var row = item.ROWS[0];
            Assert.Equal("PB00002", row.CHILD_ID);
            Assert.Equal(12.3f, row.QTY);
            Assert.Equal("123", row.UOM);
        }

        [Fact]
        public void OkeiException_Create_ShouldReturnEmpty()
        {
            var bom = new Bom
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Артикул родителя", Value = "PB00001" },
                    new Attribute{Name = "Версия", Value = "2" },
                    new Attribute{Name = "Наименование", Value = "Наименование" },
                    new Attribute{Name = "Кол-во выходного изделия", Value = "2.3" },
                },
                Components = new[]
                {
                    new Component
                    {
                        Attributes = new []
                        {
                            new Attribute{Name = "Артикул", Value = "PB00002" },
                            new Attribute{Name = "Количество", Value = "12" },
                            new Attribute{Name = "Единица измерения количества", Value = "шт." },
                        }
                    }
                }
            };

            var okeiServiceMock = new Mock<IOkeiService>();
            okeiServiceMock.Setup(t => t.GetOkeiCodeAsync(It.IsAny<string>())).Throws(new OkeiCodeNotFoundException("Wrong"));

            var bomFactory = new BomsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);

            var boms = new List<Bom> {bom};

            var items = bomFactory.Create(ref boms);

            Assert.Empty(items);
            Assert.Empty(boms);
        }

        [Fact]
        public void EmptyChildId_Create_ShouldReturnEmptyBom()
        {
            var bom = new Bom
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Артикул родителя", Value = "PB00001" },
                    new Attribute{Name = "Версия", Value = "2" },
                    new Attribute{Name = "Наименование", Value = "Наименование" },
                    new Attribute{Name = "Кол-во выходного изделия", Value = "2.3" },
                },
                Components = new[]
                {
                    new Component
                    {
                        Attributes = new []
                        {
                            new Attribute{Name = "Количество", Value = "12" },
                            new Attribute{Name = "Единица измерения количества", Value = "шт." },
                        }
                    }
                }
            };

            var okeiServiceMock = new Mock<IOkeiService>();
            okeiServiceMock.Setup(t => t.GetOkeiCodeAsync(It.IsAny<string>())).ReturnsAsync(() => "123");

            var bomFactory = new BomsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);

            var boms = new List<Bom> { bom };

            var items = bomFactory.Create(ref boms);

            Assert.Empty(items);
            Assert.Empty(boms);
        }
    }
}
