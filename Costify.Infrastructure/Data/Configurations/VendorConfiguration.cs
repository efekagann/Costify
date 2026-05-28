using Costify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Costify.Infrastructure.Data.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.ContactPerson).HasMaxLength(100);
        builder.Property(v => v.Phone).HasMaxLength(20);
        builder.Property(v => v.Email).HasMaxLength(200);
        builder.Property(v => v.TaxNumber).HasMaxLength(20);
        builder.Property(v => v.CurrentBalance).HasPrecision(18, 2);
    }
}
