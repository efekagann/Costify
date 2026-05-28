using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Costify.Infrastructure.Repositories;
using Costify.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Costify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentBusinessService, CurrentBusinessService>();

        services.AddDbContext<CostifyDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);

            // Geliştirme ortamında detaylı EF hataları için
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<IStockCountRepository, StockCountRepository>();

        return services;
    }
}
