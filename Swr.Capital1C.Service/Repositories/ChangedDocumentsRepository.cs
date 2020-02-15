using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Infrastructure;
using Swr.Capital1C.Service.Logger;
using Swr.Capital1C.Service.Settings.Model;

namespace Swr.Capital1C.Service.Repositories
{
    public class ChangedDocumentsRepository
    {
        private readonly NLog.Logger _logger = LogHelper.Instance.GetLogger("Репозиторий измененных документов");

        private readonly ICommonSettings _commonSettings;

        public ChangedDocumentsRepository(ICommonSettings commonSettings)
        {
            _commonSettings = commonSettings;
        }

        public List<ChangedDocument> GetDocuments(CatalogTypeEnum catalogType)
        {
            string catalogName;
            CatalogDefinition ivDefinition;

            switch (catalogType)
            {
                case CatalogTypeEnum.Nomenclature:
                    catalogName = "Номенклатура";
                    ivDefinition = _commonSettings.NomenclatureDefinition;
                    break;
                case CatalogTypeEnum.Specification:
                    catalogName = "Спецификация";
                    ivDefinition = _commonSettings.BomDefinition;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(catalogType), catalogType, null);
            }

            _logger.Debug($"Получение измененных документов справочника '{catalogName}'...");

            var changedDocuments = new List<ChangedDocument>();

            var queryBuilder = new QueryBuilder();

            var query = queryBuilder.ChangedDocumentsByVariableQuery(ivDefinition, new []
            {
                _commonSettings.ArticleVariableName, _commonSettings.IsServiceVariableName
            });

            _logger.Debug(query);

            var dataTable = ExecuteQuery(query, _commonSettings.PdmDbConnection.ConnectionString());

            foreach (DataRow dataTableRow in dataTable.Rows)
            {
                var document = new ChangedDocument((int)dataTableRow["Идентификатор документа"], (string)dataTableRow["Имя конфигурации документа"], (int)dataTableRow["Версия документа"])
                {
                    Article = (string)dataTableRow[_commonSettings.ArticleVariableName],
                    IsService = dataTableRow[_commonSettings.IsServiceVariableName] == DBNull.Value ? string.Empty : (string)dataTableRow[_commonSettings.IsServiceVariableName]
                };

                changedDocuments.Add(document);
            }

            _logger.Info($"Получили '{changedDocuments.Count}' измененных документа справочника '{catalogName}'");

            return changedDocuments;
        }

        private DataTable ExecuteQuery(string query, string connectionString)
        {
            using (var sqlConnection = new SqlConnection { ConnectionString = connectionString })
            {
                sqlConnection.Open();

                using (var command = new SqlCommand(query, sqlConnection) { CommandType = CommandType.Text })
                {
                    using (var sqlDataAdapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();

                        sqlDataAdapter.Fill(dataTable);

                        return dataTable;
                    }
                }
            }
        }
    }
}
