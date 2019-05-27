using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using hitmanstat.us.Models;

/*
 * https://github.com/mahiya/PolluxWeb.AzureTableStore/blob/5abce43b90de76f65e7b42894fa486c9b9d43a74/AzureTableStore.cs
 * https://github.com/serandvaraco/unespacioparanet/blob/09e4dbee1cbbe1fd18b3c3cb6f29a996a86a779d/Azure/AzureStorage/AzureStorage.Manager/Table/TableStorage.cs
 * https://github.com/TahirNaushad/Fiver.Azure.Table/blob/c2934c75bb1c4d41f52108734b2c96b3388e7272/Fiver.Azure.Table/AzureTableStorage.cs
 * https://stackoverflow.com/questions/24234350/how-to-execute-an-azure-table-storage-query-async-client-version-4-0-1
 */

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

        public async Task<List<EventEntity>> GetTablePartitionEntitiesAsync(CloudTable table, string partitionKey, int takeCount)
        {
            var entities = new List<EventEntity>();
            var query = new TableQuery<EventEntity>()
                .Where(
                    TableQuery.GenerateFilterCondition(
                        "PartitionKey", QueryComparisons.Equal, partitionKey))
                .OrderBy("RowKey")
                .Take(takeCount);

            TableContinuationToken token = null;

            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                entities.AddRange(segment.Results);
                token = (entities.Count < takeCount) ? segment.ContinuationToken : null;
            }
            while (token != null);

            return entities;
        }

        public async Task InsertEntityAsync(CloudTable table, EventEntity entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            await table.ExecuteAsync(insertOperation);
        }
    }
}
