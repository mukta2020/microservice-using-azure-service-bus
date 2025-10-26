using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MicroserviceAzureSB.Models;

namespace MicroserviceAzureSB.Services;

public class ServiceBusPublisher
{
    private readonly ServiceBusClient _client;
    private readonly IConfiguration _config;
    private readonly string _topic;

    public ServiceBusPublisher(IConfiguration config)
    {
        _config = config;
        var conn = _config["ServiceBus:ConnectionString"] ?? throw new ArgumentNullException("ServiceBus:ConnectionString");
        _client = new ServiceBusClient(conn);
        _topic = _config["ServiceBus:OrderTopic"] ?? "orders-topic";
    }

    public async Task PublishOrderPlacedAsync(OrderPlacedEvent evt)
    {
        var sender = _client.CreateSender(_topic);
        var json = JsonSerializer.Serialize(evt);
        var msg = new ServiceBusMessage(json)
        {
            Subject = "OrderPlaced",
            MessageId = evt.OrderId
        };
        await sender.SendMessageAsync(msg);
    }

    public ServiceBusClient Client => _client;
}
