using Microsoft.EntityFrameworkCore;
using OrderFlow.Web.Data;
using OrderFlow.Web.Models;
using OrderFlow.Web.ViewModels;

namespace OrderFlow.Web.Services;

public class OrderService
{
    private readonly OrderFlowDbContext _dbContext;

    public OrderService(OrderFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Order> GetAll()
    {
        return _dbContext.Orders
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public Order? GetById(Guid id)
    {
        return _dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefault(x => x.Id == id);
    }

    public Order CreateOrder(CreateOrderViewModel model)
    {
        if (model.Items == null || model.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "Please add at least one product to the order.");
        }

        var orderItems = new List<OrderItem>();

        foreach (var requestedItem in model.Items)
        {
            var product = _dbContext.Products
                .FirstOrDefault(x => x.Id == requestedItem.ProductId);

            if (product == null)
            {
                throw new InvalidOperationException(
                    $"Product {requestedItem.ProductId} was not found.");
            }

            if (requestedItem.Quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Quantity must be greater than zero.");
            }

            if (product.AvailableStock < requestedItem.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for {product.Name}.");
            }

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = requestedItem.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * requestedItem.Quantity
            });
        }

        // Reduce inventory after all requested items are validated.
        foreach (var item in orderItems)
        {
            var product = _dbContext.Products
                .First(x => x.Id == item.ProductId);

            product.AvailableStock -= item.Quantity;
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),

            OrderNumber =
                $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}",

            CustomerName = model.CustomerName.Trim(),

            CustomerEmail = model.CustomerEmail.Trim(),

            Items = orderItems,

            TotalAmount = orderItems.Sum(x => x.TotalPrice),

            Status = OrderStatus.Pending,

            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);

        _dbContext.SaveChanges();

        return order;
    }

    public async Task ProcessOrderAsync(Guid id)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null)
        {
            throw new InvalidOperationException(
                "Order was not found.");
        }

        if (order.Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException(
                "Completed orders cannot be processed again.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Cancelled orders cannot be processed.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Order cannot be processed because its current status is {order.Status}.");
        }

        order.Status = OrderStatus.Processing;

        await _dbContext.SaveChangesAsync();

        // Temporary local processing simulation.
        // Kafka consumer will replace this later.
        await Task.Delay(1500);

        order.Status = OrderStatus.Completed;
        order.ProcessedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public void CancelOrder(Guid id)
    {
        var order = _dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefault(x => x.Id == id);

        if (order == null)
        {
            throw new InvalidOperationException(
                "Order was not found.");
        }

        if (order.Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException(
                "Completed orders cannot be cancelled.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Order is already cancelled.");
        }

        if (order.Status == OrderStatus.Processing)
        {
            throw new InvalidOperationException(
                "An order being processed cannot be cancelled.");
        }

        foreach (var item in order.Items)
        {
            var product = _dbContext.Products
                .FirstOrDefault(x => x.Id == item.ProductId);

            if (product != null)
            {
                product.AvailableStock += item.Quantity;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;

        _dbContext.SaveChanges();
    }
}