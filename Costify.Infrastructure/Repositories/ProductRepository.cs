using Costify.Domain.Entities;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Costify.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(CostifyDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync() =>
        await _dbSet
            .Include(p => p.Unit)
            .Include(p => p.Category)
            .Where(p => p.CurrentStock <= p.MinimumStock)
            .OrderBy(p => (double)p.CurrentStock)
            .ToListAsync();

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId) =>
        await _dbSet
            .Include(p => p.Unit)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();

    public async Task<Product?> GetWithDetailsAsync(int id) =>
        await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.StockTransactions.OrderByDescending(t => t.TransactionDate).Take(10))
            .FirstOrDefaultAsync(p => p.Id == id);
}
