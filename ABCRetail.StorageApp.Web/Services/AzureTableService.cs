using Azure.Data.Tables;
using ABCRetail.StorageApp.Web.Models;

namespace ABCRetail.StorageApp.Web.Services
{
    public interface IAzureTableService
    {
        Task AddCustomerAsync(CustomerProfile customer);
        Task<List<CustomerProfile>> GetAllCustomersAsync();
        Task<CustomerProfile?> GetCustomerAsync(string rowKey);
        Task AddProductAsync(ProductInventory product);
        Task<List<ProductInventory>> GetAllProductsAsync();
        Task<ProductInventory?> GetProductAsync(string rowKey);
        Task UpdateProductStockAsync(string rowKey, int newStock);
    }

    public class AzureTableService : IAzureTableService
    {
        private readonly TableClient _customerTable;
        private readonly TableClient _productTable;
        private readonly ILogger<AzureTableService> _logger;

        public AzureTableService(IConfiguration configuration, ILogger<AzureTableService> logger)
        {
            _logger = logger;
            var connectionString = configuration["AzureStorage:ConnectionString"];
            
            var customerTableName = configuration["AzureStorage:TableNames:CustomerProfiles"];
            var productTableName = configuration["AzureStorage:TableNames:ProductInventory"];

            _customerTable = new TableClient(connectionString, customerTableName);
            _productTable = new TableClient(connectionString, productTableName);

            _customerTable.CreateIfNotExists();
            _productTable.CreateIfNotExists();
        }

        public async Task AddCustomerAsync(CustomerProfile customer)
        {
            customer.PartitionKey = "CUSTOMER";
            customer.RowKey = Guid.NewGuid().ToString();
            customer.RegistrationDate = DateTime.UtcNow;
            await _customerTable.AddEntityAsync(customer);
        }

        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            var customers = new List<CustomerProfile>();
            await foreach (var customer in _customerTable.QueryAsync<CustomerProfile>())
            {
                customers.Add(customer);
            }
            return customers;
        }

        public async Task<CustomerProfile?> GetCustomerAsync(string rowKey)
        {
            try
            {
                var response = await _customerTable.GetEntityAsync<CustomerProfile>("CUSTOMER", rowKey);
                return response.Value;
            }
            catch
            {
                return null;
            }
        }

        public async Task AddProductAsync(ProductInventory product)
        {
            product.PartitionKey = "PRODUCT";
            product.RowKey = Guid.NewGuid().ToString();
            product.LastUpdated = DateTime.UtcNow;
            await _productTable.AddEntityAsync(product);
        }

        public async Task<List<ProductInventory>> GetAllProductsAsync()
        {
            var products = new List<ProductInventory>();
            await foreach (var product in _productTable.QueryAsync<ProductInventory>())
            {
                products.Add(product);
            }
            return products;
        }

        public async Task<ProductInventory?> GetProductAsync(string rowKey)
        {
            try
            {
                var response = await _productTable.GetEntityAsync<ProductInventory>("PRODUCT", rowKey);
                return response.Value;
            }
            catch
            {
                return null;
            }
        }

        public async Task UpdateProductStockAsync(string rowKey, int newStock)
        {
            var product = await GetProductAsync(rowKey);
            if (product != null)
            {
                product.StockQuantity = newStock;
                product.LastUpdated = DateTime.UtcNow;
                await _productTable.UpdateEntityAsync(product, product.ETag, TableUpdateMode.Replace);
            }
        }
    }
}