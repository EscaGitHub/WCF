using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Xunit;
using Attribute = Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In.Attribute;

namespace UnitTests
{
    public class NomenclatureValidationTest
    {
        [Fact]
        public void TrueFields_Run_ShouldValidateNomenclature()
        {
            var nomenclature = new Nomenclature
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Артикул", Value = "PB00001" },
                    new Attribute{Name = "Наименование", Value = "Деталь 1" },
                    new Attribute{Name = "Обозначение", Value = "SWR.000.001" },
                    new Attribute{Name = "Единица измерения", Value = "мм" },
                    new Attribute{Name = "Раздел спецификации", Value = "Детали" },
                    new Attribute{Name = "Вид номенклатуры", Value = "Ролики_2" },
                    new Attribute{Name = "Признак готовой продукции", Value = "1" },
                    new Attribute{Name = "Вид воспроизводства", Value = "Покупное" },
                    new Attribute{Name = "Материал", Value = "сталь" },
                    new Attribute{Name = "Плотность", Value = "0.5" },
                    new Attribute{Name = "Масса", Value = "13.2" },
                    new Attribute{Name = "Площадь поверхности", Value = "3.2" },
                    new Attribute{Name = "Количество гибов", Value = "3" },
                    new Attribute{Name = "Общая длина", Value = "44.7" },
                    new Attribute{Name = "Общая ширина", Value = "" }, // empty value
                    new Attribute{Name = "Общая толщина", Value = "4.7" },
                    new Attribute{Name = "Длина реза", Value = "1.7" },
                }
            };

            var validator = new NomenclatureValidation();
            validator.Run(nomenclature, "Артикул");
        }

        [Fact]
        public void SectionWrong_Run_ShouldValidateNomenclature()
        {
            var nomenclature = new Nomenclature
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Артикул", Value = "PB00001" },
                    new Attribute{Name = "Наименование", Value = "Деталь 1" },
                    new Attribute{Name = "Единица измерения", Value = "мм" },
                    new Attribute{Name = "Раздел спецификации", Value = "Детали3" },
                    new Attribute{Name = "Вид номенклатуры", Value = "Ролики_2" },
                    new Attribute{Name = "Признак готовой продукции", Value = "1" },
                    new Attribute{Name = "Вид воспроизводства", Value = "Покупное" },
                    new Attribute{Name = "Материал", Value = "сталь" },
                }
            };

            var validator = new NomenclatureValidation();

            var message = Assert.Throws<NomenclatureIsInvalidException>(() => validator.Run(nomenclature, "Артикул")).Message;
            Assert.True(message != null && message.Contains("Атрибут 'Раздел спецификации':\r\nЗначение не соответствует списку."));
        }

        [Fact]
        public void RequiredArticle_Run_ShouldValidateNomenclature()
        {
            var nomenclature = new Nomenclature
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Наименование", Value = "Деталь 1" },
                    new Attribute{Name = "Единица измерения", Value = "мм" },
                    new Attribute{Name = "Раздел спецификации", Value = "Детали" },
                    new Attribute{Name = "Вид номенклатуры", Value = "Ролики_2" },
                    new Attribute{Name = "Признак готовой продукции", Value = "1" },
                    new Attribute{Name = "Вид воспроизводства", Value = "Покупное" },
                    new Attribute{Name = "Материал", Value = "сталь" },
                }
            };

            var validator = new NomenclatureValidation();

            var message = Assert.Throws<NomenclatureIsInvalidException>(() => validator.Run(nomenclature, "Артикул")).Message;
            Assert.True(message != null && message.Contains("Атрибут 'Артикул':\r\nЗначение не задано"));
        }

        [Fact]
        public void WrongArticleLength_Run_ShouldValidateNomenclature()
        {
            var nomenclature = new Nomenclature
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Артикул", Value = "PB0000111111111" },
                    new Attribute{Name = "Наименование", Value = "Деталь 1" },
                    new Attribute{Name = "Единица измерения", Value = "мм" },
                    new Attribute{Name = "Раздел спецификации", Value = "Детали" },
                    new Attribute{Name = "Вид номенклатуры", Value = "Ролики_2" },
                    new Attribute{Name = "Признак готовой продукции", Value = "1" },
                    new Attribute{Name = "Вид воспроизводства", Value = "Покупное" },
                    new Attribute{Name = "Материал", Value = "сталь" },
                }
            };

            var validator = new NomenclatureValidation();

            var message = Assert.Throws<NomenclatureIsInvalidException>(() => validator.Run(nomenclature, "Артикул")).Message;
            Assert.True(message != null && message.Contains("Атрибут 'Артикул':\r\nДлина текста не соответствует ограничению."));
        }
    }
}
