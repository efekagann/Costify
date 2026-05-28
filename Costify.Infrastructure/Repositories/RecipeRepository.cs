using Costify.Domain.Entities;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Costify.Infrastructure.Repositories;

public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    public RecipeRepository(CostifyDbContext context) : base(context) { }

    public async Task<Recipe?> GetWithIngredientsAsync(int id) =>
        await _dbSet
            .Include(r => r.Ingredients)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Unit)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IReadOnlyList<Recipe>> GetAllWithIngredientsAsync() =>
        await _dbSet
            .Include(r => r.Ingredients)
                .ThenInclude(i => i.Product)
            .OrderBy(r => r.Category)
            .ThenBy(r => r.Name)
            .ToListAsync();
}
