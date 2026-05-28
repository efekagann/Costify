using Costify.Domain.Entities;

namespace Costify.Domain.Interfaces.Repositories;

public interface IStockCountRepository : IRepository<StockCount>
{
    Task<StockCount?> GetWithItemsAsync(int id);
}
