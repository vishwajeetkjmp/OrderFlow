using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace OrderFlow.Web.Services;

public class OrderMessageSender
{
    private readonly IConfiguration _configuration;

    public OrderMessageSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendOrderForProcessingAsync(
        Guid orderId,
        string orderNumber)
    {
        var connectionString =
            _configuration["ServiceBus:ConnectionString"];

        var queueName =
            _configuration["ServiceBus:QueueName"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Service Bus connection string is missing.");
        }

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InvalidOperationException(
                "Service Bus queue name is missing.");
        }

        await using var client =
                new ServiceBusClient(
                    connectionString,
                    new ServiceBusClientOptions
                    {
                        TransportType = ServiceBusTransportType.AmqpWebSockets
                    });

        ServiceBusSender sender =
            client.CreateSender(queueName);

        var payload = new
        {
            OrderId = orderId,
            OrderNumber = orderNumber,
            EventType = "OrderProcessingRequested",
            CreatedAt = DateTime.UtcNow
        };

        string json =
            JsonSerializer.Serialize(payload);

        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = "OrderProcessingRequested",
            MessageId = orderId.ToString()
        };

        await sender.SendMessageAsync(message);
    }
}