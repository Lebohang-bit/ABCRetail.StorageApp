using Azure;
using Azure.Data.Tables;

namespace ABCRetail.StorageApp.Web.Models
{
    public class CustomerProfile : ITableEntity
    {
        public string PartitionKey { get; set; } = "CUSTOMER";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public string? CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? ShippingAddress { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string? LoyaltyTier { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}