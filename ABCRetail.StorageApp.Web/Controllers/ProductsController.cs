using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ABCRetail.StorageApp.Web.Models;
using ABCRetail.StorageApp.Web.Services;
using System;
using System.Globalization;  // ✅ ADD THIS
using System.Threading.Tasks;

namespace ABCRetail.StorageApp.Web.Controllers
{
    [Route("Products")]
    public class ProductsController : Controller
    {
        private readonly IAzureTableService _tableService;
        private readonly IAzureBlobService _blobService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IAzureTableService tableService, IAzureBlobService blobService, ILogger<ProductsController> logger)
        {
            _tableService = tableService;
            _blobService = blobService;
            _logger = logger;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var products = await _tableService.GetAllProductsAsync();
            foreach (var product in products)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    product.ImageUrl = _blobService.GetImageUrl(product.ImageUrl);
                }
            }
            return View(products);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInventory product, IFormFile? imageFile)
        {
            // ✅ FIX: Parse price with proper decimal culture
            if (Request.Form.TryGetValue("Price", out var priceValue))
            {
                var priceStr = priceValue.ToString().Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedPrice))
                {
                    product.Price = parsedPrice;
                }
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var blobName = await _blobService.UploadImageAsync(imageFile);
                    product.ImageUrl = blobName;
                }

                await _tableService.AddProductAsync(product);
                TempData["Success"] = $"Product {product.ProductName} added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var product = await _tableService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ProductInventory product, IFormFile? imageFile)
        {
            if (id != product.RowKey)
            {
                return NotFound();
            }

            // ✅ FIX: Parse price with proper decimal culture
            if (Request.Form.TryGetValue("Price", out var priceValue))
            {
                var priceStr = priceValue.ToString().Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedPrice))
                {
                    product.Price = parsedPrice;
                }
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var blobName = await _blobService.UploadImageAsync(imageFile);
                    product.ImageUrl = blobName;
                }

                product.PartitionKey = "PRODUCT";
                product.LastUpdated = DateTime.UtcNow;

                var existing = await _tableService.GetProductAsync(id);
                if (existing != null)
                {
                    product.ETag = existing.ETag;
                    product.Timestamp = existing.Timestamp;
                }

                await _tableService.UpdateProductStockAsync(id, product.StockQuantity);
                TempData["Success"] = $"Product {product.ProductName} updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
    }
}