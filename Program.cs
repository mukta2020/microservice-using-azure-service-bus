using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MicroserviceAzureSB.Services;
using MicroserviceAzureSB.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// repositories
builder.Services.AddSingleton<OrderRepository>();
builder.Services.AddSingleton<InventoryRepository>();

// Azure Service Bus client & services
builder.Services.AddSingleton<ServiceBusPublisher>();
builder.Services.AddSingleton<ServiceBusProcessorBackgroundService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<ServiceBusProcessorBackgroundService>());

builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<InventoryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
