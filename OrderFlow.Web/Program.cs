using OrderFlow.Web.Services;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Web.Data;
using OrderFlow.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<OrderFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderFlowDatabase")));
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<OrderMessageSender>();
builder.Services.AddHostedService<OrderProcessingConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<OrderFlowDbContext>();

    if (!dbContext.Products.Any())
    {
        dbContext.Products.AddRange(
            new Product
            {
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 75000,
                AvailableStock = 10
            },
            new Product
            {
                Name = "Smartphone",
                Description = "5G smartphone",
                Price = 45000,
                AvailableStock = 15
            },
            new Product
            {
                Name = "Headphones",
                Description = "Wireless headphones",
                Price = 5000,
                AvailableStock = 25
            },
            new Product
            {
                Name = "Monitor",
                Description = "27-inch monitor",
                Price = 25000,
                AvailableStock = 8
            },
            new Product
            {
                Name = "Keyboard",
                Description = "Mechanical keyboard",
                Price = 4000,
                AvailableStock = 20
            }
        );

        dbContext.SaveChanges();
    }
}

app.Run();
