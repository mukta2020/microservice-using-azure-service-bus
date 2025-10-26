using MicroserviceAzureSB.Models;

namespace MicroserviceAzureSB.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Order order);
    Task<Order?> GetOrderAsync(string id);
}

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(Order order);
}

public interface IInventoryService
{
    Task<bool> ReduceStockAsync(string productId, int quantity);
}
