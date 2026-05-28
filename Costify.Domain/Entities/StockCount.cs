using Costify.Domain.Entities.Base;
using Costify.Domain.Enums;

namespace Costify.Domain.Entities;

public class StockCount : BaseEntity, IBusinessEntity
{
    public int BusinessId { get; set; }
    public DateTime CountDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public StockCountStatus Status { get; set; } = StockCountStatus.InProgress;

    public Business Business { get; set; } = null!;
    public ICollection<StockCountItem> Items { get; set; } = new List<StockCountItem>();
}
