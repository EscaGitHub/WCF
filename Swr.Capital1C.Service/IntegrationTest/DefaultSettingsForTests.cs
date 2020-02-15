using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Settings.Model;

namespace IntegrationTest
{
    public static class DefaultSettingsForTests
    {
        public static CommonSettings Get()
        {
            return new CommonSettings
            {
                NomenclatureDefinition = new NomenclatureDefinition
                {
                    StatusVariableName = "_Статус_передачи_номенклатуры",
                    StatusVariableValue = "Готова к передаче в 1С:УПП"
                },
                BomDefinition = new BomDefinition
                {
                    StatusVariableName = "_Статус_передачи_спецификации",
                    StatusVariableValue = "Готова к передаче в 1С:УПП"
                },
                ArticleVariableName = "_Артикул_для_1С",
                IsServiceVariableName = "спр_Служебная_запись",
                PdmDbConnection = new DbConnection
                {
                    Server = "pdmwe46",
                    DataBase = "PDM",
                    UserName = "sa",
                    Password = "qwe123PDM",
                    Timeout = 40
                },
                NomenclatureCatalogServiceConnection = new CatalogServiceConnection
                {
                    Address = "http://localhost:5004/api/v2/nomenclatures/",
                    IdentityAddress = "http://localhost:5004/api/account",
                    Login = "clientuser",
                    Password = "12345"
                },
                BomCatalogServiceConnection = new CatalogServiceConnection
                {
                    Address = "http://localhost:5000/api/v2/engineering-boms/",
                    IdentityAddress = "http://localhost:5000/api/account",
                    Login = "clientuser",
                    Password = "12345"
                }
            };
        }
    }
}
