using Microsoft.AspNetCore.Mvc;
using MicroserviceAzureSB.Models;
using MicroserviceAzureSB.Services;
using MicroserviceAzureSB.Repositories;

namespace MicroserviceAzureSB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly OrderRepository _orderRepo;
    private readonly InventoryRepository _inventoryRepo;

    public OrdersController(OrderService orderService, OrderRepository orderRepo, InventoryRepository inventoryRepo)
    {
        _orderService = orderService;
        _orderRepo = orderRepo;
        _inventoryRepo = inventoryRepo;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Order order)
    {
        var created = await _orderService.CreateOrderAsync(order);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var order = await _orderService.GetOrderAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpGet("products")]
    public async Task<IActionResult> ListProducts()
    {
        var list = await _inventoryRepo.ListAsync();
        return Ok(list);
    }
}
