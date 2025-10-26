using MicroserviceAzureSB.Models;

namespace MicroserviceAzureSB.Services;

public class PaymentService
{
    public Task<bool> ProcessPaymentAsync(Order order)
    {
        // Simulate success if total < 1000
        bool success = order.Total < 1000m;
        return Task.FromResult(success);
    }
}
