using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Repositories;
using Xunit;
using Xunit.Extensions.Ordering;

namespace IntegrationTest
{
    public class ExportedItemRepositoryTest
    {
        private const string ConnectionString = "Data Source=(local);Initial Catalog=Capital;Integrated Security=SSPI";

        [Fact, Order(0)]
        public async void PrepareTable()
        {
            var repository = new ExportedItemRepository(ConnectionString);

            repository.TruncateTable();

            await repository.AddAsync(new ExportedItem("123", new DateTime(2019, 10, 02, 15, 52, 0, 0), 0));
            await repository.AddAsync(new ExportedItem("124", DateTime.Now, 0));
        }

        [Fact]
        public async void Add_ShouldAddNewItem()
        {
            var repository = new ExportedItemRepository(ConnectionString);

            await repository.AddAsync(new ExportedItem(Guid.NewGuid().ToString(), DateTime.Now, 0));
        }

        [Fact]
        public async void FindByIdAndType_ShouldReturnItem()
        {
            var repository = new ExportedItemRepository(ConnectionString);

            var item = await repository.GetByIdAndTypeAsync("123", 0);

            Assert.Equal("123", item.Id);
            Assert.Equal(new DateTime(2019, 10, 02, 15, 52, 0, 0), item.ModifiedDate);
        }

        [Fact]
        public async void ItemNotExist_FindById_ShouldReturnNull()
        {
            var repository = new ExportedItemRepository(ConnectionString);

            var item = await repository.GetByIdAndTypeAsync("123456789", 0);

            Assert.Null(item);
        }

        [Fact]
        public async void Update_ShouldUpdateItem()
        {
            var repository = new ExportedItemRepository(ConnectionString);

            await repository.UpdateAsync(new ExportedItem("124", DateTime.Now, 0));
        }
    }
}
