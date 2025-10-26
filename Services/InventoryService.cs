using MicroserviceAzureSB.Repositories;

namespace MicroserviceAzureSB.Services;

public class InventoryService
{
    private readonly InventoryRepository _repo;

    public InventoryService(InventoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> ReduceStockAsync(string productId, int quantity)
    {
        return await _repo.DecreaseStockAsync(productId, quantity);
    }
}
