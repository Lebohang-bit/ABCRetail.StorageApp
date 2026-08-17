using Azure;
using Azure.Data.Tables;

namespace ABCRetail.StorageApp.Web.Models
{
    public class ProductInventory : ITableEntity
    {
        public string PartitionKey { get; set; } = "PRODUCT";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public DateTime? LastUpdated { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}