using Costify.Domain.Entities;

namespace Costify.Domain.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetLowStockProductsAsync();
    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId);
    Task<Product?> GetWithDetailsAsync(int id);
}
