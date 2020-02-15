using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service;
using Swr.Capital1C.Service.Domain.Services.Boms.Models;
using Swr.Capital1C.Service.Domain.Services.Boms.Models.In;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Infrastructure.Boms;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Settings;
using Swr.Capital1C.Service.Settings.Model;
using Xunit;
using Xunit.Extensions.Ordering;
using Attribute = Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In.Attribute;


namespace IntegrationTest
{
    public class ServiceTest
    {
        private CommonSettings _settings;

        public ServiceTest()
        {
            CommonSettingsController.Dispose();
            _settings = CommonSettingsController.Settings;
        }

        private const string Skip = "";

        [Fact]
        public async void IsAvailable_ShouldReturnTrue()
        {
            var service = new Service(null);

            var result = await service.IsAvailableAsync();

            Assert.True(result);
        }

        [Fact(Skip = Skip), Order(1)]
        public async void GetNomenclatures_ShouldReturnNomenclatures()
        {
            var settings = CommonSettingsController.Settings;
            settings.NomenclatureDefinition.FolderDefinitions = new List<FolderDefinition>
            {
                new FolderDefinition
                {
                    FolderPaths = new List<string> { @"\Проекты\Тесты\009 Полная цепочка СП\" }
                }
            };

            var guid = Guid.NewGuid().ToString();

            var request = new ITEMS_REQ
            {
                HEADER = new HEADER
                {
                    MSGID = guid,
                    TIMESTAMP = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                }
            };

            var service = new Service(settings);
            var result = await service.GetNomenclaturesAsync(request);

            Assert.Equal(guid, result.HEADER.MSGID);
            Assert.Equal(3, result.ITEMS.Length);

            var item = result.ITEMS.FirstOrDefault(t => t.ID == "P0000037172");
            Assert.NotNull(item);
            Assert.Equal("Детали", item.BOM_PART);
            Assert.Equal("Ручка SWR.019.010", item.NAME);
            Assert.Equal(60.3f, item.WEIGHT);

            ReturnCardStatusValue();
        }

        private async void ReturnCardStatusValue()
        {
            using (var service = new NomenclatureCatalogService(CommonSettingsController.Settings.NomenclatureCatalogServiceConnection))
            {
                var ids = new List<string>
                {
                    "32594:По умолчанию:10",
                    "32595:По умолчанию:14",
                    "32595:-01:14"
                };

                foreach (var id in ids)
                {
                    var nom = new Nomenclature
                    {
                        Id = id,
                        Attributes = new[]
                        {
                            new Attribute
                            {
                                Name = "Статус передачи номенклатуры",
                                Value = "Готова к передаче в 1С:УПП"
                            }
                        }
                    };

                    await service.UpdateNomenclatureAsync(nom);
                }
            }
        }

        [Fact]
        private async void ReturnBomCardStatusValue()
        {
            using (var service = new BomCatalogService(CommonSettingsController.Settings.BomCatalogServiceConnection))
            {
                var ids = new List<string>
                {
                    "32595:По умолчанию:14",
                    "32595:-01:14"
                };

                foreach (var id in ids)
                {
                    var nom = new Bom
                    {
                        Id = id,
                        Attributes = new[]
                        {
                            new Attribute
                            {
                                Name = "Статус передачи спецификации",
                                Value = "Готова к передаче в 1С:УПП"
                            }
                        }
                    };

                    await service.UpdateBomAsync(nom);
                }
            }
        }


        [Fact(Skip = Skip)]
        [Order(2)]
        public async void SetNomenclaturesResponse_ShouldUpdateStatusAttribute()
        {
            var settings = CommonSettingsController.Settings;
            settings.NomenclatureDefinition.FolderDefinitions = new List<FolderDefinition>
            {
                new FolderDefinition
                {
                    FolderPaths = new List<string> { @"\Проекты\Тесты\007 Обновление переменной\" }
                }
            };

            var guid = Guid.NewGuid().ToString();

            var request = new ITEM_RESULTS_REQ
            {
                HEADER = new HEADER
                {
                    MSGID = guid,
                    TIMESTAMP = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                },
                ITEM_RESULTS = new []
                {
                    new ITEM_RESULT
                    {
                        ID = "P0000036686",
                        RESULT = 0,
                        MESSAGE = "Ошибка номенклатуры"
                    }, 
                }
            };

            var service = new Service(settings);
            var result = await service.SetNomenclaturesResponseAsync(request);

            // SWR.007.030.SLDPRT, По умолчанию, Прочее, Статус и описание ошибки
        }

        [Fact]
        [Order(3)]
        public async void GetBoms_ShouldReturnBoms()
        {
            var settings = CommonSettingsController.Settings;
            settings.NomenclatureDefinition.FolderDefinitions = new List<FolderDefinition>
            {
                new FolderDefinition
                {
                    FolderPaths = new List<string> { @"\Проекты\Тесты\009 Полная цепочка СП\" }
                }
            };

            var guid = Guid.NewGuid().ToString();

            var request = new BOMS_REQ
            {
                HEADER = new HEADER
                {
                    MSGID = guid,
                    TIMESTAMP = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                }
            };

            var service = new Service(settings);
            var result = await service.GetBomsAsync(request);

            Assert.Equal(guid, result.HEADER.MSGID);
            Assert.Equal(2,result.BOMS.Length);

            var item = result.BOMS.FirstOrDefault(t => t.PARENT_ID == "P0000037175");
            Assert.NotNull(item);
            Assert.Equal("Ручка", item.BOM_NAME);
            Assert.Equal("E", item.VERSION);
            Assert.Equal(1, item.QTY_BOM);
            Assert.Single(item.ROWS);
            var row = item.ROWS[0];
            Assert.Equal("P0000037172", row.CHILD_ID);
            Assert.Equal("796", row.UOM);
            Assert.Equal(1, row.QTY);

            item = result.BOMS.FirstOrDefault(t => t.PARENT_ID == "P0000037174");
            Assert.NotNull(item);
            Assert.Equal("Ручка длинная", item.BOM_NAME);
            Assert.Equal("E", item.VERSION);
            Assert.Equal(1, item.QTY_BOM);
            Assert.Single(item.ROWS);
            row = item.ROWS[0];
            Assert.Equal("P0000037172", row.CHILD_ID);
            Assert.Equal("796", row.UOM);
            Assert.Equal(5, row.QTY);

            ReturnBomCardStatusValue();
        }

        [Fact(Skip = Skip)]
        [Order(4)]
        public async void SetBomsResponse_ShouldUpdateStatusAttribute()
        {
            var settings = CommonSettingsController.Settings;
            settings.NomenclatureDefinition.FolderDefinitions = new List<FolderDefinition>
            {
                new FolderDefinition
                {
                    FolderPaths = new List<string> { @"\Проекты\Тесты\007 Обновление переменной\" }
                }
            };

            var guid = Guid.NewGuid().ToString();

            var request = new BOM_RESULTS_REQ
            {
                HEADER = new HEADER
                {
                    MSGID = guid,
                    TIMESTAMP = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                },
                BOM_RESULTS = new[]
                {
                    new BOM_RESULT
                    {
                        ID = "P0000036686",
                        RESULT = 0,
                        MESSAGE = "Ошибка СП"
                    },
                }
            };

            var service = new Service(settings);
            var result = await service.SetBomsResponseAsync(request);

            // SWR.007.030.SLDPRT, По умолчанию, Прочее, Статус и описание ошибки
        }

        //[Fact(Skip = Skip), Order(5)]
        public async void SetNomenclaturesResponse_ShouldSendMessageOnException()
        {
            var settings = CommonSettingsController.Settings;
            var tmpAddress = settings.NomenclatureCatalogServiceConnection.IdentityAddress;

            settings.NomenclatureCatalogServiceConnection.IdentityAddress = tmpAddress + "wrong"; // Throw Exception

            var guid = Guid.NewGuid().ToString();

            var request = new ITEM_RESULTS_REQ
            {
                HEADER = new HEADER
                {
                    MSGID = guid,
                    TIMESTAMP = DateTime.Now.ToString(CultureInfo.InvariantCulture)
                },
                ITEM_RESULTS = new[]
                {
                    new ITEM_RESULT
                    {
                        ID = "P0000036686",
                        RESULT = 0,
                        MESSAGE = "Ошибка номенклатуры"
                    },
                }
            };

            var service = new Service(settings);

            await Assert.ThrowsAsync<AggregateException>(() => service.SetNomenclaturesResponseAsync(request));

            settings.NomenclatureCatalogServiceConnection.IdentityAddress = tmpAddress;
        }
    }
}
