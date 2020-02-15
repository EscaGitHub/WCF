using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Boms;
using Swr.Capital1C.Service.Domain.Services.Boms.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Xunit;

namespace UnitTests
{
    public class BomValidationTest
    {
        [Fact]
        public void TrueFields_Run_ShouldValidateBom()
        {
            var bom = new Bom
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Артикул родителя", Value = "PB00001" },
                    new Attribute{Name = "Версия", Value = "3" },
                    new Attribute{Name = "Наименование", Value = "Ручка" },
                    new Attribute{Name = "Кол-во выходного изделия", Value = "2" },
                },
                Components = new []
                {
                    new Component {
                        Count = 3,
                        Attributes = new []
                        {
                            new Attribute {Name = "Артикул", Value = "PB00002" },
                            new Attribute {Name = "Единица измерения количества", Value = "мм" },
                        }
                    } 
                }
            };

            var validator = new BomValidation();
            validator.Run(bom);
        }

        [Fact]
        public void EmptyNotation_Run_ShouldPassBom()
        {
            var bom = new Bom
            {
                Attributes = new[]
                {
                    new Attribute{Name = "Артикул родителя", Value = "PB00001" },
                    new Attribute{Name = "Версия", Value = "3" },
                    new Attribute{Name = "Наименование", Value = "" },
                    new Attribute{Name = "Кол-во выходного изделия", Value = "2" },
                },
                Components = new[]
                {
                    new Component {
                        Count = 3,
                        Attributes = new []
                        {
                            new Attribute {Name = "Артикул", Value = "PB00002" },
                            new Attribute {Name = "Единица измерения количества", Value = "мм" },
                        }
                    }
                }
            };

            var validator = new BomValidation();
            Assert.Throws<BomIsInvalidException>(() => validator.Run(bom));
        }
    }
}
