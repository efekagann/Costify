using Costify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Costify.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.SKU).HasMaxLength(50);
        builder.Property(p => p.CurrentStock).HasPrecision(18, 4);
        builder.Property(p => p.MinimumStock).HasPrecision(18, 4);
        builder.Property(p => p.LastPurchasePrice).HasPrecision(18, 2);

        builder.HasIndex(p => new { p.BusinessId, p.SKU }).IsUnique().HasFilter("[SKU] IS NOT NULL");

        builder.HasOne(p => p.Business)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Unit)
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
