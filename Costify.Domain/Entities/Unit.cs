using Costify.Domain.Entities.Base;

namespace Costify.Domain.Entities;

public class Unit : BaseEntity
{
    public string Name { get; set; } = string.Empty;   // Kilogram, Litre, Adet...
    public string Symbol { get; set; } = string.Empty; // kg, lt, ad, pk

    // Navigation Properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
