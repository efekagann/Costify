using Costify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Costify.Infrastructure.Data.Configurations;

public class StockCountItemConfiguration : IEntityTypeConfiguration<StockCountItem>
{
    public void Configure(EntityTypeBuilder<StockCountItem> builder)
    {
        builder.Property(i => i.TheoreticalQuantity).HasPrecision(18, 4);
        builder.Property(i => i.CountedQuantity).HasPrecision(18, 4);
        builder.Ignore(i => i.Difference);
    }
}
