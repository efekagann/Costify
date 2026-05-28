using Costify.Domain.Entities.Base;

namespace Costify.Domain.Entities;

public class RecipeIngredient : BaseEntity
{
    public int RecipeId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }

    public Recipe Recipe { get; set; } = null!;
    public Product Product { get; set; } = null!;

    public decimal TotalCost => Quantity * (Product?.LastPurchasePrice ?? 0);
}
