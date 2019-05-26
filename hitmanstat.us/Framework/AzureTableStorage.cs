using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using hitmanstat.us.Models;

namespace hitmanstat.us.Framework
{
    public class AzureTableStorage
    {
        private readonly string ConnectionString;

        public AzureTableStorage(string connectionString) => ConnectionString = connectionString;

        public CloudStorageAccount CreateStorageAccountFromConnectionString() 
            => CloudStorageAccount.Parse(ConnectionString);

        public async Task<CloudTable> GetTableAsync(string tableName)
        {
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString();
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            return table;
        }

        public async Task<EventEntity> InsertEntityAsync(CloudTable table, EventEntity entity)
        {
            TableOperation insertOperation = TableOperation.Insert(entity);
            TableResult result = await table.ExecuteAsync(insertOperation);
            
            return result.Result as EventEntity;
        }
    }
}
