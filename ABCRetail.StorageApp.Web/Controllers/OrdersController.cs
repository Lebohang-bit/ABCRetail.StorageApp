using Microsoft.AspNetCore.Mvc;
using ABCRetail.StorageApp.Web.Models;
using ABCRetail.StorageApp.Web.Services;
using System;
using System.Threading.Tasks;

namespace ABCRetail.StorageApp.Web.Controllers
{
    [Route("Orders")]
    public class OrdersController : Controller
    {
        private readonly IAzureQueueService _queueService;
        private readonly IAzureTableService _tableService;
        private readonly IAzureFileService _fileService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IAzureQueueService queueService, IAzureTableService tableService, IAzureFileService fileService, ILogger<OrdersController> logger)
        {
            _queueService = queueService;
            _tableService = tableService;
            _fileService = fileService;
            _logger = logger;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var queueName = "order-processing";
            var count = await _queueService.GetQueueMessageCountAsync(queueName);
            ViewBag.QueueMessageCount = count;
            return View();
        }

        [Route("CreateOrder")]
        public async Task<IActionResult> CreateOrder()
        {
            var products = await _tableService.GetAllProductsAsync();
            ViewBag.Products = products;
            return View();
        }

        [HttpPost]
        [Route("CreateOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(OrderMessage order)
        {
            if (ModelState.IsValid)
            {
                order.OrderId = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 6)}";
                order.OrderDate = DateTime.UtcNow;
                order.Status = "Pending";

                var queueName = "order-processing";
                await _queueService.SendOrderMessageAsync(order, queueName);

                var logContent = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Order {order.OrderId} created for {order.CustomerName}. Total: R {order.TotalAmount:F2}";
                await _fileService.UploadLogAsync($"order-{order.OrderId}.log", logContent);

                TempData["Success"] = $"Order {order.OrderId} created and queued for processing!";
                return RedirectToAction(nameof(Index));
            }

            var products = await _tableService.GetAllProductsAsync();
            ViewBag.Products = products;
            return View(order);
        }

        [HttpPost]
        [Route("ProcessNextOrder")]
        public async Task<IActionResult> ProcessNextOrder()
        {
            var queueName = "order-processing";
            var order = await _queueService.ReceiveOrderMessageAsync<OrderMessage>(queueName);

            if (order != null)
            {
                order.Status = "Processing";
                var logContent = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Processing order {order.OrderId} for {order.CustomerName}. Total: R {order.TotalAmount:F2}";
                await _fileService.UploadLogAsync($"order-{order.OrderId}-processed.log", logContent);

                TempData["Success"] = $"Processing order: {order.OrderId} for {order.CustomerName}";
            }
            else
            {
                TempData["Info"] = "No orders in queue to process.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}