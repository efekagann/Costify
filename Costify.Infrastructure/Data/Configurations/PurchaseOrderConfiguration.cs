using Costify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Costify.Infrastructure.Data.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

        builder.HasIndex(o => new { o.BusinessId, o.OrderNumber }).IsUnique();

        builder.HasOne(o => o.Vendor)
            .WithMany(v => v.PurchaseOrders)
            .HasForeignKey(o => o.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.PurchaseOrder)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
