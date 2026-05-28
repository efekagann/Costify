using Costify.Domain.Entities;

namespace Costify.Domain.Interfaces.Repositories;

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetWithItemsAsync(int id);
    Task<IReadOnlyList<PurchaseOrder>> GetRecentOrdersAsync(int count = 10);
    Task<string> GenerateOrderNumberAsync();
}
