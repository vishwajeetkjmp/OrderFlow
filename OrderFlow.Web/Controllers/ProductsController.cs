using Microsoft.AspNetCore.Mvc;
using OrderFlow.Web.Services;
using OrderFlow.Web.Models;

namespace OrderFlow.Web.Controllers;

public class ProductsController : Controller
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    public IActionResult Index()
    {
        var products = _productService.GetAll();

        return View(products);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        _productService.Add(product);

        TempData["SuccessMessage"] = "Product added successfully.";

        return RedirectToAction(nameof(Index));
    }
}