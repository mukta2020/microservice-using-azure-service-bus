namespace MicroserviceAzureSB.Models;

public class OrderItem
{
    public string ProductId { get; set; } = null!;
    public int Quantity { get; set; }
}

public class Order
{
    public string Id { get; set; } = System.Guid.NewGuid().ToString();
    public string CustomerId { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string Status { get; set; } = "Created";
}
