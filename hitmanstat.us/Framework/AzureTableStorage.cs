using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using hitmanstat.us.Models;

namespace hitmanstat.us.Framework
{
    public class AzureTableStorage
    {
        private readonly string ConnectionString;

        public AzureTableStorage(string connectionString) => ConnectionString = connectionString;

        public CloudStorageAccount GetStorageAccount() => CloudStorageAccount.Parse(ConnectionString);

        public async Task<CloudTable> GetTableAsync(string tableName)
        {
            var storageAccount = GetStorageAccount();
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            return table;
        }

        public async Task InsertEntityAsync(CloudTable table, EventEntity entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            await table.ExecuteAsync(insertOperation);
        }
    }
}
