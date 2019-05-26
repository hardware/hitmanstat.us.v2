using Microsoft.Azure.Cosmos.Table;

namespace hitmanstat.us.Models
{
    public class EventEntity : TableEntity
    {
        public EventEntity()
        { }

        public EventEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Service { get; set; }
        public string Status { get; set; }
    }
}
