using Costify.Domain.Entities.Base;

namespace Costify.Domain.Entities;

public class Vendor : BaseEntity, IBusinessEntity
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public decimal? CurrentBalance { get; set; } = 0;   // Cari bakiye
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public Business Business { get; set; } = null!;
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
