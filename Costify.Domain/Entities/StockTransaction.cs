using Costify.Domain.Entities.Base;
using Costify.Domain.Enums;

namespace Costify.Domain.Entities;

public class StockTransaction : BaseEntity, IBusinessEntity
{
    public int BusinessId { get; set; }
    public int ProductId { get; set; }
    public StockTransactionType Type { get; set; }
    public decimal Quantity { get; set; }            // (+) giriş, (-) çıkış
    public decimal UnitCost { get; set; } = 0;
    public int? PurchaseOrderId { get; set; }        // Satın almaya bağlıysa
    public string? Notes { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Business Business { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public PurchaseOrder? PurchaseOrder { get; set; }
}
