using Costify.Domain.Entities.Base;
using Costify.Domain.Enums;

namespace Costify.Domain.Entities;

public class PurchaseOrder : BaseEntity, IBusinessEntity
{
    public int BusinessId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public decimal TotalAmount { get; set; } = 0;
    public string? Notes { get; set; }

    // Navigation Properties
    public Business Business { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
