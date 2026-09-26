using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using OrderFlow.Web.Models;
using System.Text.Json;

namespace OrderFlow.Web.Services;

public sealed class OrderProcessingConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderProcessingConsumer> _logger;

    private ServiceBusClient? _client;
    private ServiceBusProcessor? _processor;

    public OrderProcessingConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<OrderProcessingConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
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

        _client = new ServiceBusClient(
            connectionString,
            new ServiceBusClientOptions
            {
                TransportType =
                    ServiceBusTransportType.AmqpWebSockets
            });

        _processor = _client.CreateProcessor(
            queueName,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1
            });

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);

        _logger.LogInformation(
            "OrderProcessingConsumer started. Queue: {QueueName}",
            queueName);

        try
        {
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "OrderProcessingConsumer stopping.");
        }
    }

    private async Task ProcessMessageAsync(
        ProcessMessageEventArgs args)
    {
        try
        {
            string body = args.Message.Body.ToString();

            _logger.LogInformation(
                "Received Service Bus message. MessageId: {MessageId}",
                args.Message.MessageId);

            var message =
                JsonSerializer.Deserialize<OrderProcessingMessage>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (message == null)
            {
                throw new InvalidOperationException(
                    "Failed to deserialize OrderProcessingMessage.");
            }

            using IServiceScope scope =
                _scopeFactory.CreateScope();

            var orderService =
                scope.ServiceProvider
                    .GetRequiredService<OrderService>();

            await orderService
                .CompleteOrderProcessingAsync(
                    message.OrderId);

            await args.CompleteMessageAsync(
                args.Message);

            _logger.LogInformation(
                "Successfully processed order {OrderNumber}",
                message.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed processing Service Bus message. MessageId: {MessageId}",
                args.Message.MessageId);

            try
            {
                await args.AbandonMessageAsync(
                    args.Message);
            }
            catch (Exception abandonException)
            {
                _logger.LogError(
                    abandonException,
                    "Failed to abandon Service Bus message.");
            }
        }
    }

    private Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus error. Source={Source}, Entity={Entity}, Namespace={Namespace}",
            args.ErrorSource,
            args.EntityPath,
            args.FullyQualifiedNamespace);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Stopping OrderProcessingConsumer...");

        if (_processor != null)
        {
            await _processor.StopProcessingAsync(
                cancellationToken);

            _processor.ProcessMessageAsync -=
                ProcessMessageAsync;

            _processor.ProcessErrorAsync -=
                ProcessErrorAsync;

            await _processor.DisposeAsync();
        }

        if (_client != null)
        {
            await _client.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}