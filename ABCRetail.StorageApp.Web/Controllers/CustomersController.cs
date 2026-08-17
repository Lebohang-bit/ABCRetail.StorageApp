using Microsoft.AspNetCore.Mvc;
using ABCRetail.StorageApp.Web.Models;
using ABCRetail.StorageApp.Web.Services;
using System;
using System.Threading.Tasks;

namespace ABCRetail.StorageApp.Web.Controllers
{
    [Route("Customers")]
    public class CustomersController : Controller
    {
        private readonly IAzureTableService _tableService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(IAzureTableService tableService, ILogger<CustomersController> logger)
        {
            _tableService = tableService;
            _logger = logger;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetAllCustomersAsync();
            return View(customers);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile customer)
        {
            if (ModelState.IsValid)
            {
                await _tableService.AddCustomerAsync(customer);
                TempData["Success"] = $"Customer {customer.FullName} added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        [Route("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var customer = await _tableService.GetCustomerAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }
    }
}