using Microsoft.AspNetCore.Mvc;
using ABCRetail.StorageApp.Web.Services;
using System;
using System.Threading.Tasks;

namespace ABCRetail.StorageApp.Web.Controllers
{
    [Route("Logs")]
    public class LogsController : Controller
    {
        private readonly IAzureFileService _fileService;
        private readonly ILogger<LogsController> _logger;

        public LogsController(IAzureFileService fileService, ILogger<LogsController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var files = await _fileService.GetAllLogFilesAsync();
            return View(files);
        }

        [Route("ViewLog")]
        public async Task<IActionResult> ViewLog(string fileName)
        {
            var content = await _fileService.DownloadLogAsync(fileName);
            if (content == null)
            {
                return NotFound();
            }

            ViewBag.FileName = fileName;
            ViewBag.Content = content;
            return View();
        }

        [HttpPost]
        [Route("DeleteLog")]
        public async Task<IActionResult> DeleteLog(string fileName)
        {
            var result = await _fileService.DeleteLogAsync(fileName);
            if (result)
            {
                TempData["Success"] = $"Log file '{fileName}' deleted.";
            }
            else
            {
                TempData["Error"] = $"Failed to delete log file '{fileName}'.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("GenerateSystemLog")]
        public async Task<IActionResult> GenerateSystemLog()
        {
            var content = $@"
[SYSTEM LOG - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}]
System Status: Healthy
Active Queues: order-processing, inventory-updates
Memory Usage: {GC.GetTotalMemory(false) / 1024 / 1024} MB
====================================================";

            var fileName = $"system-log-{DateTime.UtcNow:yyyyMMddHHmmss}.log";
            await _fileService.UploadLogAsync(fileName, content);
            TempData["Success"] = $"System log '{fileName}' generated.";
            return RedirectToAction(nameof(Index));
        }
    }
}