using Costify.Domain.Entities;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Costify.Infrastructure.Repositories;

public class VendorRepository : Repository<Vendor>, IVendorRepository
{
    public VendorRepository(CostifyDbContext context) : base(context) { }

    public async Task<Vendor?> GetWithOrdersAsync(int id) =>
        await _dbSet
            .Include(v => v.PurchaseOrders.OrderByDescending(o => o.OrderDate).Take(20))
                .ThenInclude(o => o.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Unit)
            .FirstOrDefaultAsync(v => v.Id == id);
}
