using Costify.Domain.Entities;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Costify.Infrastructure.Repositories;

public class StockCountRepository : Repository<StockCount>, IStockCountRepository
{
    public StockCountRepository(CostifyDbContext context) : base(context) { }

    public async Task<StockCount?> GetWithItemsAsync(int id) =>
        await _dbSet
            .Include(sc => sc.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Unit)
            .FirstOrDefaultAsync(sc => sc.Id == id);
}
