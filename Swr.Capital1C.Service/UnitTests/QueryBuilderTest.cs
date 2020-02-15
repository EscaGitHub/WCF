using System;
using System.Collections.Generic;
using Swr.Capital1C.Service.Infrastructure;
using Swr.Capital1C.Service.Settings.Model;
using Xunit;

namespace UnitTests
{
    public class QueryBuilderTest
    {
        [Fact]
        public void ChangedDocumentsByVariableQuery_ShouldReturnQuery()
        {
            var builder = new QueryBuilder();

            var nomenclatureDefinition = new NomenclatureDefinition
            {
                FolderDefinitions = new List<FolderDefinition>
                {
                    new FolderDefinition
                    {
                        FolderPaths = new List<string>
                        {
                            @"\Проекты\", @"\Справочники\"
                        }
                    }
                },
                StatusVariableName = "_Статус_передачи_номенклатуры",
                StatusVariableValue = "В разработке"
            };

            var systemAttributes = new[] {"спр_Служебная_запись", "_Артикул_для_1С"};

            var query = builder.ChangedDocumentsByVariableQuery(nomenclatureDefinition, systemAttributes);

            Assert.Equal(Properties.Resources.ChangedNomenclatureQuery, query);
        }

        [Fact]
        public void SomeCatalogDefinitions_ChangedDocumentsByVariableQuery_ShouldReturnQuery()
        {
            var builder = new QueryBuilder();

            var nomenclatureDefinition = new NomenclatureDefinition
            {
                FolderDefinitions = new List<FolderDefinition>
                {
                    new FolderDefinition
                    {
                        FolderPaths = new List<string>
                        {
                            @"\Проекты\", @"\Справочники\"
                        }
                    },
                    new FolderDefinition
                    {
                        FolderPaths = new List<string>
                        {
                            @"\Проекты\"
                        }
                    }
                },
                StatusVariableName = "_Статус_передачи_номенклатуры",
                StatusVariableValue = "В разработке"
            };

            var systemAttributes = new[] { "спр_Служебная_запись", "_Артикул_для_1С" };

            var query = builder.ChangedDocumentsByVariableQuery(nomenclatureDefinition, systemAttributes);

            Assert.Equal(Properties.Resources.ChangedNomenclatureQuery, query);
        }
    }
}
