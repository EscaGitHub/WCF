using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Swr.Capital1C.Okei;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Settings;
using Swr.Capital1C.Service.Settings.Model;
using Xunit;
using Attribute = Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In.Attribute;

namespace UnitTests
{
    public class ItemsFactoryTest
    {
        [Fact]
        public void Standard_Create_ShouldReturnItems()
        {
            var nomenclature = new Nomenclature
            {
                Attributes = new []
                {
                    new Attribute{Name = "Артикул", Value = "PB00001" },
                    new Attribute{Name = "Наименование", Value = "Деталь 1" },
                    new Attribute{Name = "Обозначение", Value = "SWR.000.001" },
                    new Attribute{Name = "Единица измерения", Value = "мм" },
                    new Attribute{Name = "Раздел спецификации", Value = "Детали" },
                    new Attribute{Name = "Вид номенклатуры", Value = "Ролики_2" },
                    new Attribute{Name = "Признак готовой продукции", Value = "11" },
                    new Attribute{Name = "Вид воспроизводства", Value = "Вид1" },
                    new Attribute{Name = "Материал", Value = "сталь" },
                    new Attribute{Name = "Плотность", Value = "0.5" },
                    new Attribute{Name = "Масса", Value = "13.2" },
                    new Attribute{Name = "Площадь поверхности", Value = "3.2" },
                    new Attribute{Name = "Количество гибов", Value = "3" },
                    new Attribute{Name = "Общая длина", Value = "44.7" },
                    new Attribute{Name = "Общая ширина", Value = "20.1" },
                    new Attribute{Name = "Общая толщина", Value = "4.7" },
                    new Attribute{Name = "Длина реза", Value = "1.7" },
                }
            };

            var okeiServiceMock = new Mock<IOkeiService>();
            okeiServiceMock.Setup(t => t.GetOkeiCodeAsync(It.IsAny<string>())).ReturnsAsync(() => "123");

            var itemsFactory = new ItemsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);
            var nomenclatures = new List<Nomenclature> {nomenclature};
            var item = itemsFactory.Create(ref nomenclatures)[0];

            Assert.Single(nomenclatures);

            Assert.Equal("PB00001", item.ID);
            Assert.Equal("Деталь 1 SWR.000.001", item.NAME);
            Assert.Equal ("123", item.UOM);
            Assert.Equal("Детали", item.BOM_PART);
            Assert.Equal("21 Полуфабрикат собствен", item.TYPE);
            Assert.Equal("11", item.IS_PRODUCT);
            Assert.Equal("Вид1", item.PURCHASED);
            Assert.Equal("сталь", item.MATERIAL);
            Assert.Equal(0.5f, item.DENSITY);
            Assert.Equal(13.2f, item.WEIGHT);
            Assert.Equal(3.2f, item.AREA);
            Assert.Equal(3, item.BEND);
            Assert.Equal(44.7f, item.LENGTH);
            Assert.Equal(20.1f, item.WIDTH);
            Assert.Equal(4.7f, item.THICK);
            Assert.Equal(1.7f, item.LENGTH_CUT);
        }

        [Fact]
        public void NomenclatureView_Create_ShouldChangeTypeAndName()
        {
            var nomenclature = new Nomenclature
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Артикул", Value = "PB00001" },
                    new Attribute{Name = "Наименование", Value = "Деталь 1" },
                    new Attribute{Name = "Обозначение", Value = "SWR.000.001" },
                    new Attribute{Name = "Раздел спецификации", Value = "материалы" },
                    new Attribute{Name = "Вид воспроизводства", Value = "Покупное" },
                    new Attribute{Name = "Вид номенклатуры", Value = "Ролики_2" },
                    new Attribute{Name = "Признак готовой продукции", Value = "11" },
                }
            };

            var okeiServiceMock = new Mock<IOkeiService>();
            okeiServiceMock.Setup(t => t.GetOkeiCodeAsync(It.IsAny<string>())).ReturnsAsync(() => "123");

            var itemsFactory = new ItemsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);
            var nomenclatures = new List<Nomenclature> { nomenclature };
            var item = itemsFactory.Create(ref nomenclatures)[0];

            Assert.Equal("Деталь 1", item.NAME); // changed
            Assert.Equal("материалы", item.BOM_PART);
            Assert.Equal("10-01 Сырье и материалы", item.TYPE); // changed

            nomenclature.Attributes[3].Value = "материалы1"; // реагирует на Покупное
            itemsFactory = new ItemsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);
            item = itemsFactory.Create(ref nomenclatures)[0];

            Assert.Equal("Деталь 1", item.NAME); 
            Assert.Equal("материалы1", item.BOM_PART);
            Assert.Equal("10-01 Сырье и материалы", item.TYPE);

            nomenclature.Attributes[3].Value = "детали";
            itemsFactory = new ItemsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);
            item = itemsFactory.Create(ref nomenclatures)[0];

            Assert.Equal("Деталь 1 SWR.000.001", item.NAME); // changed
            Assert.Equal("детали", item.BOM_PART);
            Assert.Equal("10-01 Сырье и материалы", item.TYPE);

            nomenclature.Attributes[4].Value = "Никакое";
            nomenclature.Attributes[6].Value = "1";
            itemsFactory = new ItemsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);
            item = itemsFactory.Create(ref nomenclatures)[0];

            Assert.Equal("Деталь 1 SWR.000.001", item.NAME);
            Assert.Equal("детали", item.BOM_PART);
            Assert.Equal("43 Готовая продукция", item.TYPE); // changed
        }

        [Fact]
        public void NameByBomPart_Create_ShouldChangeName()
        {
            var nomenclature = new Nomenclature
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Артикул", Value = "PB00001" },
                    new Attribute{Name = "Наименование", Value = "Деталь 1" },
                    new Attribute{Name = "Обозначение", Value = "SWR.000.001" },
                    new Attribute{Name = "Раздел спецификации", Value = "сборочные единицы" },
                }
            };

            var okeiServiceMock = new Mock<IOkeiService>();
            okeiServiceMock.Setup(t => t.GetOkeiCodeAsync(It.IsAny<string>())).ReturnsAsync(() => "123");


            var itemsFactory = new ItemsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);
            var nomenclatures = new List<Nomenclature> { nomenclature };
            var item = itemsFactory.Create(ref nomenclatures)[0];

            Assert.Equal("Деталь 1 SWR.000.001", item.NAME);
            Assert.Equal("сборочные единицы", item.BOM_PART);
        }

        [Fact]
        public void EmptyArticle_Create_ShouldReturnEmpty()
        {
            var nomenclature = new Nomenclature
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Наименование", Value = "Деталь 1" },
                    new Attribute{Name = "Обозначение", Value = "SWR.000.001" },
                    new Attribute{Name = "Раздел спецификации", Value = "сборочные единицы" },
                }
            };

            var okeiServiceMock = new Mock<IOkeiService>();
            okeiServiceMock.Setup(t => t.GetOkeiCodeAsync(It.IsAny<string>())).ReturnsAsync(() => "123");


            var itemsFactory = new ItemsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);
            var nomenclatures = new List<Nomenclature> { nomenclature };
            var items = itemsFactory.Create(ref nomenclatures);

            Assert.Empty(items);
            Assert.Empty(nomenclatures);
        }

        [Fact]
        public void WrongOkei_Create_ShouldReturnEmpty()
        {
            var nomenclature = new Nomenclature
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Артикул", Value = "PB00001" },
                    new Attribute{Name = "Наименование", Value = "Деталь 1" },
                    new Attribute{Name = "Обозначение", Value = "SWR.000.001" },
                    new Attribute{Name = "Раздел спецификации", Value = "сборочные единицы" },
                    new Attribute{Name = "Единица измерения", Value = "WrongValue" },
                }
            };

            var okeiServiceMock = new Mock<IOkeiService>();
            okeiServiceMock.Setup(t => t.GetOkeiCodeAsync(It.IsAny<string>())).Throws(new OkeiCodeNotFoundException(It.IsAny<string>()));


            var itemsFactory = new ItemsFactory(CommonSettingsController.GetDefault(), okeiServiceMock.Object);
            var nomenclatures = new List<Nomenclature> { nomenclature };
            var items = itemsFactory.Create(ref nomenclatures);

            Assert.Empty(items);
            Assert.Empty(nomenclatures);
        }
    }
}
