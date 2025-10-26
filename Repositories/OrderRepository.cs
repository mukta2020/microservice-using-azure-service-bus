using MicroserviceAzureSB.Models;

namespace MicroserviceAzureSB.Repositories;

public class OrderRepository
{
    private readonly Dictionary<string, Order> _orders = new();

    public Task AddAsync(Order order)
    {
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<Order?> GetAsync(string id)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task UpdateAsync(Order order)
    {
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
}
