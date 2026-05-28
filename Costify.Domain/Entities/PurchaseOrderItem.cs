using Costify.Domain.Entities.Base;

namespace Costify.Domain.Entities;

public class PurchaseOrderItem : BaseEntity
{
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public decimal ReceivedQuantity { get; set; } = 0;

    // Navigation Properties
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
