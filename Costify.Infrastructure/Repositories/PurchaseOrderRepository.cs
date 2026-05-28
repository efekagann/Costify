using Costify.Domain.Entities;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Costify.Infrastructure.Repositories;

public class PurchaseOrderRepository : Repository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(CostifyDbContext context) : base(context) { }

    public async Task<PurchaseOrder?> GetWithItemsAsync(int id) =>
        await _dbSet
            .Include(o => o.Vendor)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Unit)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IReadOnlyList<PurchaseOrder>> GetRecentOrdersAsync(int count = 10) =>
        await _dbSet
            .Include(o => o.Vendor)
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync();

    public async Task<string> GenerateOrderNumberAsync()
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        var count = await _context.PurchaseOrders
            .IgnoreQueryFilters()
            .CountAsync(o => o.CreatedAt.Year == year && o.CreatedAt.Month == month);

        return $"SIP-{year}{month:D2}-{(count + 1):D4}";
    }
}
