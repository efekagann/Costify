using Costify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Costify.Infrastructure.Data.Configurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Quantity).HasPrecision(18, 4);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.ReceivedQuantity).HasPrecision(18, 4);

        // Hesaplanmış alan — veritabanına yazılmaz
        builder.Ignore(i => i.TotalPrice);

        builder.HasOne(i => i.Product)
            .WithMany(p => p.PurchaseOrderItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
