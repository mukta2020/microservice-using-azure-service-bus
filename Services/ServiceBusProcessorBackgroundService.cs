using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MicroserviceAzureSB.Models;
using MicroserviceAzureSB.Services;
using System.Text.Json;

namespace MicroserviceAzureSB.Services;

public class ServiceBusProcessorBackgroundService : BackgroundService
{
    private readonly ILogger<ServiceBusProcessorBackgroundService> _logger;
    private readonly ServiceBusClient _client;
    private readonly IConfiguration _config;
    private readonly PaymentService _paymentService;
    private readonly InventoryService _inventoryService;
    private ServiceBusProcessor? _orderProcessor;
    private ServiceBusProcessor? _paymentProcessor;

    public ServiceBusProcessorBackgroundService(ILogger<ServiceBusProcessorBackgroundService> logger, ServiceBusPublisher publisher, IConfiguration config, PaymentService paymentService, InventoryService inventoryService)
    {
        _logger = logger;
        _client = publisher.Client;
        _config = config;
        _paymentService = paymentService;
        _inventoryService = inventoryService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var orderTopic = _config["ServiceBus:OrderTopic"] ?? "orders-topic";
        var paymentSub = _config["ServiceBus:PaymentSubscription"] ?? "payment-sub";
        var inventorySub = _config["ServiceBus:InventorySubscription"] ?? "inventory-sub";
        var paymentTopic = _config["ServiceBus:PaymentTopic"] ?? "payment-topic";

        // Processor to handle OrderPlaced messages -> simulate payment and publish to payment topic
        _orderProcessor = _client.CreateProcessor(orderTopic, paymentSub, new ServiceBusProcessorOptions { AutoCompleteMessages = false });
        _orderProcessor.ProcessMessageAsync += OrderMessageHandler;
        _orderProcessor.ProcessErrorAsync += ErrorHandler;
        await _orderProcessor.StartProcessingAsync(stoppingToken);

        // Processor to handle PaymentProcessed messages (from payment-topic subscription) to update inventory
        _paymentProcessor = _client.CreateProcessor(paymentTopic, inventorySub, new ServiceBusProcessorOptions { AutoCompleteMessages = false });
        _paymentProcessor.ProcessMessageAsync += PaymentMessageHandler;
        _paymentProcessor.ProcessErrorAsync += ErrorHandler;
        await _paymentProcessor.StartProcessingAsync(stoppingToken);

        _logger.LogInformation("ServiceBus processors started.");
    }

    private async Task OrderMessageHandler(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        try
        {
            var evt = JsonSerializer.Deserialize<OrderPlacedEvent>(body);
            if (evt != null)
            {
                _logger.LogInformation("Processing payment for order {OrderId}", evt.OrderId);
                var order = new Order { Id = evt.OrderId, CustomerId = evt.CustomerId, Items = evt.Items, Total = evt.Total };
                var success = await _paymentService.ProcessPaymentAsync(order);

                // publish PaymentProcessedEvent to payment topic
                var paymentEvt = new PaymentProcessedEvent { OrderId = evt.OrderId, Success = success };
                var sender = _client.CreateSender(_config["ServiceBus:PaymentTopic"] ?? "payment-topic");
                var json = JsonSerializer.Serialize(paymentEvt);
                await sender.SendMessageAsync(new ServiceBusMessage(json) { Subject = "PaymentProcessed", MessageId = paymentEvt.OrderId });
            }
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling order message");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private async Task PaymentMessageHandler(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        try
        {
            var evt = JsonSerializer.Deserialize<PaymentProcessedEvent>(body);
            if (evt != null && evt.Success)
            {
                _logger.LogInformation("Payment succeeded for {OrderId} - update inventory", evt.OrderId);
                // In real impl we would fetch order items; simplified: log only
            }
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling payment message");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "ServiceBus Error: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_orderProcessor != null) await _orderProcessor.StopProcessingAsync(cancellationToken);
        if (_paymentProcessor != null) await _paymentProcessor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
