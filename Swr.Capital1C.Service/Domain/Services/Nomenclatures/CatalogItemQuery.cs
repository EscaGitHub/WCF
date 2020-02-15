using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;

namespace Swr.Capital1C.Service.Domain.Services.Nomenclatures
{
    public class CatalogItemQuery : ICatalogItemQuery
    {
        private readonly string _connectionString;
        private readonly string _vendorCodeVariableName;

        public CatalogItemQuery(string connectionString, string vendorCodeVariableName)
        {
            _connectionString = connectionString;
            _vendorCodeVariableName = vendorCodeVariableName;
        }

        public async Task<CatalogItemId> FindByArticleAsync(string vendorCode)
        {
            var query = BuildQuery();

            var parameters = new[]
            {
                new SqlParameter("vendorCode", SqlDbType.NVarChar)
                {
                    Value = vendorCode
                }
            };

            var dataTable = await ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            if (dataTable.Rows.Count > 1)
                throw new FoundMoreThanOneCatalogItemsException();

            var dataTableRow = dataTable.Rows[0];

            return new CatalogItemId((int) dataTableRow["Идентификатор документа"],
                (string) dataTableRow["Имя конфигурации документа"], 
                (int) dataTableRow["Версия документа"]);
        }

        private string BuildQuery()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"declare @vendorCodeVariableId as int = (select VariableID from Variable where VariableName = N'{_vendorCodeVariableName}' and IsDeleted = 0);");

            builder.AppendLine(@"select
docs.DocumentID as N'Идентификатор документа'
,docs.LatestRevisionNo as N'Версия документа'
,docConfigs.ConfigurationName as N'Имя конфигурации документа'
from VariableValue as vendorCodeVariableValues
inner join DocumentConfiguration as docConfigs on docConfigs.ConfigurationID = vendorCodeVariableValues.ConfigurationID
inner join Documents as docs on docs.DocumentID = vendorCodeVariableValues.DocumentID
where
docs.Deleted = 0
and vendorCodeVariableValues.VariableID = @vendorCodeVariableId
and vendorCodeVariableValues.ValueText = @vendorCode
and vendorCodeVariableValues.RevisionNo = (select max(RevisionNo) from VariableValue where DocumentID = docs.DocumentId 
                                        and ConfigurationID = docConfigs.ConfigurationID
										and VariableID = @vendorCodeVariableId)");

            return builder.ToString();
        }

        private async Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[] parameters)
        {
            using (var sqlConnection = new SqlConnection { ConnectionString = _connectionString })
            {
                await sqlConnection.OpenAsync();

                using (var command = new SqlCommand(query, sqlConnection) { CommandType = CommandType.Text })
                {
                    command.Parameters.AddRange(parameters);

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