namespace OrderFlow.Web.Models;

public class OrderItem
{
    public int Id { get; set; }

    public Guid OrderId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public Order? Order { get; set; }
}