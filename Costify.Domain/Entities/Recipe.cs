using Costify.Domain.Entities.Base;

namespace Costify.Domain.Entities;

public class Recipe : BaseEntity, IBusinessEntity
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal SellingPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public Business Business { get; set; } = null!;
    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();

    public decimal TotalCost => Ingredients.Sum(i => i.TotalCost);
    public decimal FoodCostPercentage => SellingPrice > 0 ? Math.Round(TotalCost / SellingPrice * 100, 1) : 0;
}
