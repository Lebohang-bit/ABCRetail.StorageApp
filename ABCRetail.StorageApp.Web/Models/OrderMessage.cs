namespace ABCRetail.StorageApp.Web.Models
{
    public class OrderMessage
    {
        public string? OrderId { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public List<OrderItem>? Items { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; }
    }

    public class OrderItem
    {
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}