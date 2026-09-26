namespace OrderFlow.Web.Models;

public class OrderProcessingMessage
{
    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
