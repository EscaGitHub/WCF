using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Swr.Capital1C.Service.Repositories
{
    public class ExportedItemRepository : IExportedItemRepository
    {
        private readonly string _connectionString;
        private readonly string _table;

        public ExportedItemRepository(string connectionString)
        {
            _connectionString = connectionString;
            _table = "ExportedDocument";
        }

        public async Task AddAsync(ExportedItem item)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync($"INSERT INTO {_table} VALUES(@id, @modifiedDate, @documentType)",
                    new { id = item.Id, modifiedDate = item.ModifiedDate, documentType = item.DocumentType });
            }
        }

        public async Task UpdateAsync(ExportedItem item)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync($"UPDATE {_table} SET ModifiedDate = @modifiedDate WHERE ItemId = @id and DocumentType = @documentType",
                    new { id = item.Id, modifiedDate = item.ModifiedDate, documentType = item.DocumentType });
            }
        }

        public async Task<ExportedItem> GetByIdAndTypeAsync(string id, int documentType)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var item = await connection.QueryFirstOrDefaultAsync<ExportedItemDto>($"SELECT * FROM {_table} WHERE ItemId = @id and DocumentType = @documentType", new { id, documentType });
                return item == null ? null : new ExportedItem(item.ItemId, item.ModifiedDate, item.DocumentType);
            }
        }

        public async void TruncateTable()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync($"TRUNCATE TABLE {_table}");
            }
        }

        private class ExportedItemDto
        {
            public string ItemId { get; set; }

            public DateTime ModifiedDate { get; set; }

            public int DocumentType { get; set; }
        }
    }
}
