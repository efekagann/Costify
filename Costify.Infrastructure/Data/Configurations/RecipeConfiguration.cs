using Costify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Costify.Infrastructure.Data.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.Category).HasMaxLength(100);
        builder.Property(r => r.SellingPrice).HasPrecision(18, 2);

        builder.Ignore(r => r.TotalCost);
        builder.Ignore(r => r.FoodCostPercentage);
    }
}
