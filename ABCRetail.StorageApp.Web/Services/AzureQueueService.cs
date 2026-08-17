using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ABCRetail.StorageApp.Web.Services
{
    public interface IAzureQueueService
    {
        Task SendOrderMessageAsync<T>(T message, string queueName);
        Task<T?> ReceiveOrderMessageAsync<T>(string queueName);
        Task<int> GetQueueMessageCountAsync(string queueName);
        Task DeleteMessageAsync(string queueName, string messageId, string popReceipt);
    }

    public class AzureQueueService : IAzureQueueService
    {
        private readonly Dictionary<string, QueueClient> _queueClients;
        private readonly ILogger<AzureQueueService> _logger;

        public AzureQueueService(IConfiguration configuration, ILogger<AzureQueueService> logger)
        {
            _logger = logger;
            _queueClients = new Dictionary<string, QueueClient>();
            var connectionString = configuration["AzureStorage:ConnectionString"];

            var orderQueueName = configuration["AzureStorage:QueueNames:OrderProcessing"];
            var inventoryQueueName = configuration["AzureStorage:QueueNames:InventoryUpdates"];

            if (!string.IsNullOrEmpty(orderQueueName))
            {
                var client = new QueueClient(connectionString, orderQueueName);
                client.CreateIfNotExists();
                _queueClients[orderQueueName] = client;
                _logger.LogInformation($"Queue '{orderQueueName}' initialized.");
            }

            if (!string.IsNullOrEmpty(inventoryQueueName))
            {
                var client = new QueueClient(connectionString, inventoryQueueName);
                client.CreateIfNotExists();
                _queueClients[inventoryQueueName] = client;
                _logger.LogInformation($"Queue '{inventoryQueueName}' initialized.");
            }
        }

        public async Task SendOrderMessageAsync<T>(T message, string queueName)
        {
            try
            {
                if (!_queueClients.ContainsKey(queueName))
                {
                    throw new ArgumentException($"Queue '{queueName}' not found.");
                }

                var json = JsonSerializer.Serialize(message);
                var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                await _queueClients[queueName].SendMessageAsync(encoded);
                _logger.LogInformation($"Message sent to queue '{queueName}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message to queue '{queueName}'.");
                throw;
            }
        }

        public async Task<T?> ReceiveOrderMessageAsync<T>(string queueName)
        {
            try
            {
                if (!_queueClients.ContainsKey(queueName))
                {
                    throw new ArgumentException($"Queue '{queueName}' not found.");
                }

                var response = await _queueClients[queueName].ReceiveMessageAsync();
                if (response.Value == null)
                {
                    return default;
                }

                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(response.Value.MessageText));
                var message = JsonSerializer.Deserialize<T>(decoded);
                
                await DeleteMessageAsync(queueName, response.Value.MessageId, response.Value.PopReceipt);
                
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error receiving message from queue '{queueName}'.");
                return default;
            }
        }

        public async Task<int> GetQueueMessageCountAsync(string queueName)
        {
            try
            {
                if (!_queueClients.ContainsKey(queueName))
                {
                    return 0;
                }

                var properties = await _queueClients[queueName].GetPropertiesAsync();
                return properties.Value.ApproximateMessagesCount;
            }
            catch
            {
                return 0;
            }
        }

        public async Task DeleteMessageAsync(string queueName, string messageId, string popReceipt)
        {
            try
            {
                if (_queueClients.ContainsKey(queueName))
                {
                    await _queueClients[queueName].DeleteMessageAsync(messageId, popReceipt);
                    _logger.LogInformation($"Message deleted from queue '{queueName}'.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message from queue '{queueName}'.");
            }
        }
    }
}