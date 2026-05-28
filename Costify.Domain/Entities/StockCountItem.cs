using Costify.Domain.Entities.Base;

namespace Costify.Domain.Entities;

public class StockCountItem : BaseEntity
{
    public int StockCountId { get; set; }
    public int ProductId { get; set; }
    public decimal TheoreticalQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public decimal Difference => CountedQuantity - TheoreticalQuantity;

    public StockCount StockCount { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
