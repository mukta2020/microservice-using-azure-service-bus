using MicroserviceAzureSB.Models;

namespace MicroserviceAzureSB.Repositories;

public class InventoryRepository
{
    private readonly Dictionary<string, Product> _products = new();

    public InventoryRepository()
    {
        _products["p1"] = new Product { Id = "p1", Name = "Widget A", Stock = 10 };
        _products["p2"] = new Product { Id = "p2", Name = "Widget B", Stock = 5 };
    }

    public Task<Product?> GetProductAsync(string id)
    {
        _products.TryGetValue(id, out var p);
        return Task.FromResult(p);
    }

    public Task<bool> DecreaseStockAsync(string id, int quantity)
    {
        if (!_products.TryGetValue(id, out var p)) return Task.FromResult(false);
        if (p.Stock < quantity) return Task.FromResult(false);
        p.Stock -= quantity;
        return Task.FromResult(true);
    }

    public Task<List<Product>> ListAsync()
    {
        return Task.FromResult(_products.Values.ToList());
    }
}
