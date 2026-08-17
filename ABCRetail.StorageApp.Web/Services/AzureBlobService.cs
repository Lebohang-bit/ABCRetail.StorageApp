using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABCRetail.StorageApp.Web.Services
{
    public interface IAzureBlobService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<Stream?> DownloadImageAsync(string blobName);
        Task<List<BlobItem>> GetAllImagesAsync();
        Task<bool> DeleteImageAsync(string blobName);
        string GetImageUrl(string blobName);
    }

    public class AzureBlobService : IAzureBlobService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobService> _logger;

        public AzureBlobService(IConfiguration configuration, ILogger<AzureBlobService> logger)
        {
            _logger = logger;
            var connectionString = configuration["AzureStorage:ConnectionString"];
            var containerName = configuration["AzureStorage:BlobContainer"];

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                var blobName = $"{Guid.NewGuid()}_{file.FileName}";
                var blobClient = _containerClient.GetBlobClient(blobName);

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

                _logger.LogInformation($"Image {blobName} uploaded successfully.");
                return blobName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Blob Storage.");
                throw;
            }
        }

        public async Task<Stream?> DownloadImageAsync(string blobName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                if (await blobClient.ExistsAsync())
                {
                    var response = await blobClient.DownloadStreamingAsync();
                    return response.Value.Content;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading image {blobName}.");
                return null;
            }
        }

        public async Task<List<BlobItem>> GetAllImagesAsync()
        {
            var images = new List<BlobItem>();
            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                images.Add(blobItem);
            }
            return images;
        }

        public async Task<bool> DeleteImageAsync(string blobName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image {blobName}.");
                return false;
            }
        }

        public string GetImageUrl(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            return blobClient.Uri.ToString();
        }
    }
}