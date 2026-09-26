using Microsoft.EntityFrameworkCore;
using OrderFlow.Web.Data;
using OrderFlow.Web.Models;

namespace OrderFlow.Web.Services;

public class ProductService
{
    private readonly OrderFlowDbContext _dbContext;

    public ProductService(OrderFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Product> GetAll()
    {
        return _dbContext.Products
            .OrderBy(x => x.Id)
            .ToList();
    }

    public Product? GetById(int id)
    {
        return _dbContext.Products
            .FirstOrDefault(x => x.Id == id);
    }

    public void Add(Product product)
    {
        _dbContext.Products.Add(product);

        _dbContext.SaveChanges();
    }
}
