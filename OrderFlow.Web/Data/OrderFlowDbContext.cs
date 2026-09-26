using Microsoft.EntityFrameworkCore;
using OrderFlow.Web.Models;

namespace OrderFlow.Web.Data;

public class OrderFlowDbContext : DbContext
{
    public OrderFlowDbContext(
        DbContextOptions<OrderFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product
        modelBuilder.Entity<Product>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        // Order
        modelBuilder.Entity<Order>()
            .Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .HasIndex(x => x.OrderNumber)
            .IsUnique();

        // Order -> OrderItems
        modelBuilder.Entity<Order>()
            .HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItem
        modelBuilder.Entity<OrderItem>()
            .Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(x => x.TotalPrice)
            .HasPrecision(18, 2);
    }
}