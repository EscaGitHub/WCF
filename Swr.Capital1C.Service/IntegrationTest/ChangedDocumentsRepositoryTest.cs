using System;
using System.Collections.Generic;
using System.Linq;
using Swr.Capital1C.Service.Domain;
using Swr.Capital1C.Service.Repositories;
using Swr.Capital1C.Service.Settings.Model;
using Xunit;

namespace IntegrationTest
{
    public class ChangedDocumentsRepositoryTest
    {
        [Fact]
        public void GetDocuments_ShouldReturnChangedNomenclature()
        {
            var settings = DefaultSettingsForTests.Get();

            settings.NomenclatureDefinition.FolderDefinitions = new List<FolderDefinition>
            {
                new FolderDefinition
                {
                    FolderPaths = new List<string>
                    {
                        @"\Проекты\Тесты\001 Получение измененной\"
                    }
                }
            };

            var repository = new ChangedDocumentsRepository(settings);

            var documents = repository.GetDocuments(CatalogTypeEnum.Nomenclature);

            Assert.Equal(4, documents.Count);
            Assert.Equal(2, documents.Count(t => t.ConfigurationName == "@"));
            Assert.Equal(2, documents.Count(t => t.ConfigurationName == "По умолчанию"));
        }

        [Fact]
        public void ShareFile_GetDocuments_ShouldReturnChangedNomenclature()
        {
            var settings = DefaultSettingsForTests.Get();

            settings.NomenclatureDefinition.FolderDefinitions = new List<FolderDefinition>
            {
                new FolderDefinition
                {
                    FolderPaths = new List<string>
                    {
                        @"\Проекты\Тесты\003 Шареный файл\"
                    }
                }
            };

            var repository = new ChangedDocumentsRepository(settings);

            var documents = repository.GetDocuments(CatalogTypeEnum.Nomenclature);

            Assert.Equal(2, documents.Count);
            Assert.Equal(1, documents.Count(t => t.ConfigurationName == "@"));
            Assert.Equal(1, documents.Count(t => t.ConfigurationName == "По умолчанию"));
        }

        [Fact]
        public void AnotherVersion_GetDocuments_ShouldReturnChangedNomenclature()
        {
            var settings = DefaultSettingsForTests.Get();

            settings.NomenclatureDefinition.FolderDefinitions = new List<FolderDefinition>
            {
                new FolderDefinition
                {
                    FolderPaths = new List<string>
                    {
                        @"\Проекты\Тесты\004 Версия больше чем утв\"
                    }
                }
            };

            var repository = new ChangedDocumentsRepository(settings);

            var documents = repository.GetDocuments(CatalogTypeEnum.Nomenclature);

            Assert.Equal(2, documents.Count);
            Assert.Equal(1, documents.Count(t => t.ConfigurationName == "@"));
            Assert.Equal(1, documents.Count(t => t.ConfigurationName == "По умолчанию"));
            Assert.Equal(2, documents[0].Version);
            Assert.Equal(2, documents[1].Version);
            Assert.Equal("P0000036648", documents[0].Article);
            Assert.Equal("P0000036647", documents[1].Article);
        }

        [Fact]
        public void GetDocuments_ShouldReturnChangedSpecification()
        {
            var settings = DefaultSettingsForTests.Get();

            settings.BomDefinition.FolderDefinitions = new List<FolderDefinition>
            {
                new FolderDefinition
                {
                    FolderPaths = new List<string>
                    {
                        @"\Проекты\Тесты\008 Получение сборки\"
                    }
                }
            };

            var repository = new ChangedDocumentsRepository(settings);

            var documents = repository.GetDocuments(CatalogTypeEnum.Specification);

            Assert.Equal(10, documents.Count);

            var assembly = documents.FindAll(t => t.Id == 32580);
            Assert.Equal(3, assembly.Count);
        }
    }
}
