namespace OrderFlow.Web.Models;

public class Order
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public List<OrderItem> Items { get; set; } = [];

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? CancelledAt { get; set; }
}