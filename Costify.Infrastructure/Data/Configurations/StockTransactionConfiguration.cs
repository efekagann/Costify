using Costify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Costify.Infrastructure.Data.Configurations;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Quantity).HasPrecision(18, 4);
        builder.Property(t => t.UnitCost).HasPrecision(18, 2);

        builder.HasOne(t => t.Product)
            .WithMany(p => p.StockTransactions)
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.PurchaseOrder)
            .WithMany()
            .HasForeignKey(t => t.PurchaseOrderId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
