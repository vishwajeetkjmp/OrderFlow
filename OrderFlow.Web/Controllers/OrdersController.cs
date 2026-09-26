using Microsoft.AspNetCore.Mvc;
using OrderFlow.Web.Services;
using OrderFlow.Web.ViewModels;

namespace OrderFlow.Web.Controllers;

public class OrdersController : Controller
{
    private readonly OrderService _orderService;
    private readonly ProductService _productService;
    private readonly OrderMessageSender _orderMessageSender;

    public OrdersController(
    OrderService orderService,
    ProductService productService,
    OrderMessageSender orderMessageSender)
    {
        _orderService = orderService;
        _productService = productService;
        _orderMessageSender = orderMessageSender;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var orders = _orderService.GetAll();

        return View(orders);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var model = new CreateOrderViewModel
        {
            AvailableProducts = _productService.GetAll(),

            Items =
            [
                new CreateOrderItemViewModel()
            ]
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableProducts = _productService.GetAll();
            return View(model);
        }

        try
        {
            var order = _orderService.CreateOrder(model);

            TempData["SuccessMessage"] =
                $"Order {order.OrderNumber} created successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = order.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            model.AvailableProducts = _productService.GetAll();

            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Details(Guid id)
    {
        var order = _orderService.GetById(id);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(Guid id)
    {
        try
        {
            var order = _orderService.GetById(id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order was not found.";

                return RedirectToAction(nameof(Index));
            }

            if (order.Status != OrderFlow.Web.Models.OrderStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    $"Only Pending orders can be processed. Current status: {order.Status}";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            await _orderMessageSender.SendOrderForProcessingAsync(
                order.Id,
                order.OrderNumber);

            TempData["SuccessMessage"] =
                "Order submitted for processing successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] =
                $"Unable to submit order: {ex.Message}";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(Guid id)
    {
        try
        {
            _orderService.CancelOrder(id);

            TempData["SuccessMessage"] =
                "Order cancelled successfully. Product stock has been restored.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id });
    }
}