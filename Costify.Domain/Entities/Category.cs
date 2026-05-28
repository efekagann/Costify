using Costify.Domain.Entities.Base;

namespace Costify.Domain.Entities;

public class Category : BaseEntity, IBusinessEntity
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorHex { get; set; }

    // Navigation Properties
    public Business Business { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
