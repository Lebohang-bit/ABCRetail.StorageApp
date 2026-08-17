using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetail.StorageApp.Web.Services
{
    public interface IAzureFileService
    {
        Task UploadLogAsync(string fileName, string content);
        Task<string?> DownloadLogAsync(string fileName);
        Task<List<ShareFileItem>> GetAllLogFilesAsync();
        Task<bool> DeleteLogAsync(string fileName);
    }

    public class AzureFileService : IAzureFileService
    {
        private readonly ShareClient _shareClient;
        private readonly ShareDirectoryClient _rootDirectory;
        private readonly ILogger<AzureFileService> _logger;

        public AzureFileService(IConfiguration configuration, ILogger<AzureFileService> logger)
        {
            _logger = logger;
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var shareName = configuration["AzureStorage:FileShare"];

            var shareServiceClient = new ShareServiceClient(connectionString);
            _shareClient = shareServiceClient.GetShareClient(shareName);
            _shareClient.CreateIfNotExists();
            
            _rootDirectory = _shareClient.GetRootDirectoryClient();
        }

        public async Task UploadLogAsync(string fileName, string content)
        {
            try
            {
                var fileClient = _rootDirectory.GetFileClient(fileName);
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                
                await fileClient.CreateAsync(stream.Length);
                stream.Position = 0;
                await fileClient.UploadAsync(stream);
                
                _logger.LogInformation($"Log file '{fileName}' uploaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading log file '{fileName}'.");
                throw;
            }
        }

        public async Task<string?> DownloadLogAsync(string fileName)
        {
            try
            {
                var fileClient = _rootDirectory.GetFileClient(fileName);
                if (await fileClient.ExistsAsync())
                {
                    var response = await fileClient.DownloadAsync();
                    using var reader = new StreamReader(response.Value.Content);
                    return await reader.ReadToEndAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading log file '{fileName}'.");
                return null;
            }
        }

        public async Task<List<ShareFileItem>> GetAllLogFilesAsync()
        {
            var files = new List<ShareFileItem>();
            try
            {
                await foreach (var item in _rootDirectory.GetFilesAndDirectoriesAsync())
                {
                    files.Add(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting log files.");
            }
            return files;
        }

        public async Task<bool> DeleteLogAsync(string fileName)
        {
            try
            {
                var fileClient = _rootDirectory.GetFileClient(fileName);
                return await fileClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting log file '{fileName}'.");
                return false;
            }
        }
    }
}