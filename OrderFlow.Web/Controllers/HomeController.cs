using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Web.Models;
using OrderFlow.Web.Services;
using OrderFlow.Web.ViewModels;


namespace OrderFlow.Web.Controllers;

public class HomeController : Controller
{

    private readonly ProductService _productService;
    private readonly OrderService _orderService;

    public HomeController(ILogger<HomeController> logger, ProductService productService, OrderService orderService)
    {
        _productService = productService;
        _orderService = orderService;
    }

    public IActionResult Index()
    {
        var products = _productService.GetAll();
        var orders = _orderService.GetAll();

        var model = new DashboardViewModel
        {
            TotalProducts = products.Count,

            TotalOrders = orders.Count,

            PendingOrders = orders.Count(x => x.Status == OrderStatus.Pending),

            CompletedOrders = orders.Count(x => x.Status == OrderStatus.Completed),

            CancelledOrders = orders.Count(x => x.Status == OrderStatus.Cancelled),

            TotalOrderValue = orders.Where(x => x.Status != OrderStatus.Cancelled).Sum(x => x.TotalAmount),

            RecentOrders = orders.Take(5).ToList()
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
