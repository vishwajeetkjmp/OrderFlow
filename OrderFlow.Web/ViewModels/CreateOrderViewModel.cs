using System.ComponentModel.DataAnnotations;
using OrderFlow.Web.Models;

namespace OrderFlow.Web.ViewModels;

public class CreateOrderViewModel
{
    [Required(ErrorMessage = "Customer name is required.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string CustomerEmail { get; set; } = string.Empty;

    public List<CreateOrderItemViewModel> Items { get; set; } = [];

    // Used to populate the product dropdown
    public List<Product> AvailableProducts { get; set; } = [];
}

public class CreateOrderItemViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a product.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; } = 1;
}