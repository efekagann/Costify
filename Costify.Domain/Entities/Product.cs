using Costify.Domain.Entities.Base;

namespace Costify.Domain.Entities;

public class Product : BaseEntity, IBusinessEntity
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }                     // Stok kodu
    public int CategoryId { get; set; }
    public int UnitId { get; set; }
    public decimal CurrentStock { get; set; } = 0;
    public decimal MinimumStock { get; set; } = 0;       // Minimum stok uyarı eşiği
    public decimal LastPurchasePrice { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public Business Business { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
