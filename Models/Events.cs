namespace MicroserviceAzureSB.Models;

public class OrderPlacedEvent
{
    public string OrderId { get; set; } = null!;
    public string CustomerId { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
}

public class PaymentProcessedEvent
{
    public string OrderId { get; set; } = null!;
    public bool Success { get; set; }
}
