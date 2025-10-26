using MicroserviceAzureSB.Repositories;
using MicroserviceAzureSB.Models;
using MicroserviceAzureSB.Services;

namespace MicroserviceAzureSB.Services;

public class OrderService
{
    private readonly OrderRepository _repo;
    private readonly ServiceBusPublisher _publisher;

    public OrderService(OrderRepository repo, ServiceBusPublisher publisher)
    {
        _repo = repo;
        _publisher = publisher;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        // naive total calculation: Quantity * 10
        order.Total = order.Items.Sum(i => i.Quantity * 10m);
        await _repo.AddAsync(order);

        var evt = new OrderPlacedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Items = order.Items,
            Total = order.Total
        };

        await _publisher.PublishOrderPlacedAsync(evt);

        return order;
    }

    public Task<Order?> GetOrderAsync(string id) => _repo.GetAsync(id);
}
