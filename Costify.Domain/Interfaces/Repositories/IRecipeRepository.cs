using Costify.Domain.Entities;

namespace Costify.Domain.Interfaces.Repositories;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<Recipe?> GetWithIngredientsAsync(int id);
    Task<IReadOnlyList<Recipe>> GetAllWithIngredientsAsync();
}
