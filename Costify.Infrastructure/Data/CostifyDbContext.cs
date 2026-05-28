using System.Linq.Expressions;
using Costify.Domain.Entities;
using Costify.Domain.Entities.Base;
using Costify.Domain.Enums;
using Costify.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Costify.Infrastructure.Data;

public class CostifyDbContext : DbContext
{
    private readonly ICurrentBusinessService _currentBusiness;

    public CostifyDbContext(
        DbContextOptions<CostifyDbContext> options,
        ICurrentBusinessService currentBusiness) : base(options)
    {
        _currentBusiness = currentBusiness;
    }

    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<StockCount> StockCounts => Set<StockCount>();
    public DbSet<StockCountItem> StockCountItems => Set<StockCountItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity konfigürasyonlarını assembly'den otomatik yükle
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CostifyDbContext).Assembly);

        // =====================================================================
        // GLOBAL QUERY FILTER — Multi-Tenant izolasyonunun kalbi
        // IBusinessEntity uygulayan her entity otomatik olarak BusinessId filtresi alır.
        // Tüm LINQ sorgularına WHERE BusinessId = @currentBusinessId eklenir.
        // =====================================================================
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IBusinessEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(CostifyDbContext)
                .GetMethod(nameof(BuildTenantFilter),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            var filter = method.Invoke(null, [_currentBusiness])!;
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter((LambdaExpression)filter);
        }

        // Soft-delete global filter (IsDeleted alanı olan tüm BaseEntity)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(CostifyDbContext)
                .GetMethod(nameof(BuildSoftDeleteFilter),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            var filter = method.Invoke(null, null)!;
            // IBusinessEntity zaten BusinessId filtresi aldı, sadece soft-delete ekle
            if (!typeof(IBusinessEntity).IsAssignableFrom(entityType.ClrType))
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter((LambdaExpression)filter);
        }

        // SQL Server çoklu kaskat yolu hatasını önlemek için FK'ları Restrict yap.
        // Yalnızca parent→child ilişkileri (aşağıdaki set) Cascade kalır; diğerleri Restrict.
        var cascadeAllowed = new HashSet<(Type declaring, Type principal)>
        {
            (typeof(PurchaseOrderItem), typeof(PurchaseOrder)),
            (typeof(RecipeIngredient),  typeof(Recipe)),
            (typeof(StockCountItem),    typeof(StockCount)),
        };

        foreach (var fk in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade))
        {
            var pair = (fk.DeclaringEntityType.ClrType, fk.PrincipalEntityType.ClrType);
            if (!cascadeAllowed.Contains(pair))
                fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        SeedDefaultData(modelBuilder);
    }

    // BusinessId filtresi: p => p.BusinessId == currentBusiness.BusinessId
    private static LambdaExpression BuildTenantFilter<T>(ICurrentBusinessService svc)
        where T : BaseEntity, IBusinessEntity
    {
        Expression<Func<T, bool>> filter = entity =>
            !entity.IsDeleted && entity.BusinessId == svc.BusinessId;
        return filter;
    }

    // Soft-delete filtresi: p => !p.IsDeleted
    private static LambdaExpression BuildSoftDeleteFilter<T>()
        where T : BaseEntity
    {
        Expression<Func<T, bool>> filter = entity => !entity.IsDeleted;
        return filter;
    }

    // Audit alanlarını otomatik güncelle
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(ct);
    }

    private static void SeedDefaultData(ModelBuilder mb)
    {
        mb.Entity<Business>().HasData(new Business
        {
            Id = 1,
            Name = "Demo Kafe",
            Email = "demo@costify.app",
            IsActive = true,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        mb.Entity<Unit>().HasData(
            new Unit { Id = 1, Name = "Kilogram", Symbol = "kg", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Unit { Id = 2, Name = "Gram", Symbol = "gr", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Unit { Id = 3, Name = "Litre", Symbol = "lt", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Unit { Id = 4, Name = "Mililitre", Symbol = "ml", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Unit { Id = 5, Name = "Adet", Symbol = "ad", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Unit { Id = 6, Name = "Paket", Symbol = "pk", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Unit { Id = 7, Name = "Koli", Symbol = "kl", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        mb.Entity<Category>().HasData(
            new Category { Id = 1, BusinessId = 1, Name = "İçecek Hammaddesi", ColorHex = "#3B82F6", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 2, BusinessId = 1, Name = "Yiyecek Hammaddesi", ColorHex = "#10B981", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 3, BusinessId = 1, Name = "Ambalaj Malzemesi", ColorHex = "#F59E0B", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 4, BusinessId = 1, Name = "Temizlik Malzemesi", ColorHex = "#EF4444", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        var d = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        mb.Entity<Vendor>().HasData(
            new Vendor { Id = 1, BusinessId = 1, Name = "Kadıköy Kahve A.Ş.", ContactPerson = "Mehmet Yılmaz", Phone = "0212 555 10 01", Email = "siparis@kadikoyKahve.com.tr", TaxNumber = "1234567890", IsActive = true, CreatedAt = d },
            new Vendor { Id = 2, BusinessId = 1, Name = "Özgün Gıda Ltd.", ContactPerson = "Ayşe Kaya", Phone = "0216 555 20 02", Email = "info@ozgungida.com", IsActive = true, CreatedAt = d },
            new Vendor { Id = 3, BusinessId = 1, Name = "Ambalaj Plus", ContactPerson = "Ali Demir", Phone = "0212 555 30 03", Email = "satis@ambalajplus.com", IsActive = true, CreatedAt = d }
        );

        mb.Entity<Product>().HasData(
            new Product { Id = 1,  BusinessId = 1, Name = "Arabica Kahve Çekirdeği", SKU = "KAH-001", CategoryId = 1, UnitId = 1, CurrentStock = 28.5m,  MinimumStock = 5,   LastPurchasePrice = 485m,  IsActive = true, CreatedAt = d },
            new Product { Id = 2,  BusinessId = 1, Name = "Robusta Kahve Çekirdeği", SKU = "KAH-002", CategoryId = 1, UnitId = 1, CurrentStock = 3m,     MinimumStock = 5,   LastPurchasePrice = 320m,  IsActive = true, CreatedAt = d },
            new Product { Id = 3,  BusinessId = 1, Name = "Tam Yağlı Süt",           SKU = "SUT-001", CategoryId = 1, UnitId = 3, CurrentStock = 48m,    MinimumStock = 10,  LastPurchasePrice = 42m,   IsActive = true, CreatedAt = d },
            new Product { Id = 4,  BusinessId = 1, Name = "Krema (Barista)",          SKU = "KRE-001", CategoryId = 1, UnitId = 3, CurrentStock = 6m,     MinimumStock = 5,   LastPurchasePrice = 72m,   IsActive = true, CreatedAt = d },
            new Product { Id = 5,  BusinessId = 1, Name = "Şeker",                    SKU = "SEK-001", CategoryId = 2, UnitId = 1, CurrentStock = 22m,    MinimumStock = 5,   LastPurchasePrice = 48m,   IsActive = true, CreatedAt = d },
            new Product { Id = 6,  BusinessId = 1, Name = "Un",                       SKU = "UN-001",  CategoryId = 2, UnitId = 1, CurrentStock = 2m,     MinimumStock = 5,   LastPurchasePrice = 58m,   IsActive = true, CreatedAt = d },
            new Product { Id = 7,  BusinessId = 1, Name = "Bardak 8oz",               SKU = "AML-001", CategoryId = 3, UnitId = 5, CurrentStock = 450m,   MinimumStock = 100, LastPurchasePrice = 1.2m,  IsActive = true, CreatedAt = d },
            new Product { Id = 8,  BusinessId = 1, Name = "Bardak Kapağı",            SKU = "AML-002", CategoryId = 3, UnitId = 5, CurrentStock = 380m,   MinimumStock = 100, LastPurchasePrice = 0.8m,  IsActive = true, CreatedAt = d },
            new Product { Id = 9,  BusinessId = 1, Name = "Peçete (100'lü Paket)",    SKU = "AML-003", CategoryId = 3, UnitId = 6, CurrentStock = 25m,    MinimumStock = 5,   LastPurchasePrice = 22m,   IsActive = true, CreatedAt = d },
            new Product { Id = 10, BusinessId = 1, Name = "Çay Poşeti",               SKU = "CAY-001", CategoryId = 1, UnitId = 6, CurrentStock = 18m,    MinimumStock = 10,  LastPurchasePrice = 85m,   IsActive = true, CreatedAt = d },
            new Product { Id = 11, BusinessId = 1, Name = "Deterjan (Genel Temizlik)", SKU = "TEM-001", CategoryId = 4, UnitId = 3, CurrentStock = 4m,    MinimumStock = 2,   LastPurchasePrice = 95m,   IsActive = true, CreatedAt = d },
            new Product { Id = 12, BusinessId = 1, Name = "Soda (330ml)",             SKU = "SOD-001", CategoryId = 1, UnitId = 5, CurrentStock = 0m,     MinimumStock = 24,  LastPurchasePrice = 8.5m,  IsActive = true, CreatedAt = d }
        );

        mb.Entity<PurchaseOrder>().HasData(
            new PurchaseOrder
            {
                Id = 1, BusinessId = 1, OrderNumber = "SIP-202501-0001", VendorId = 1,
                OrderDate = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2025, 1, 12, 0, 0, 0, DateTimeKind.Utc),
                Status = OrderStatus.Received, TotalAmount = 16650m, Notes = "İlk sipariş", CreatedAt = d
            },
            new PurchaseOrder
            {
                Id = 2, BusinessId = 1, OrderNumber = "SIP-202501-0002", VendorId = 2,
                OrderDate = new DateTime(2025, 1, 22, 0, 0, 0, DateTimeKind.Utc),
                Status = OrderStatus.Ordered, TotalAmount = 1980m, CreatedAt = d
            }
        );

        mb.Entity<PurchaseOrderItem>().HasData(
            new PurchaseOrderItem { Id = 1, PurchaseOrderId = 1, ProductId = 1, Quantity = 30, UnitPrice = 485m, ReceivedQuantity = 30, CreatedAt = d },
            new PurchaseOrderItem { Id = 2, PurchaseOrderId = 1, ProductId = 3, Quantity = 50, UnitPrice = 42m,  ReceivedQuantity = 50, CreatedAt = d },
            new PurchaseOrderItem { Id = 3, PurchaseOrderId = 2, ProductId = 5, Quantity = 20, UnitPrice = 48m,  ReceivedQuantity = 0,  CreatedAt = d },
            new PurchaseOrderItem { Id = 4, PurchaseOrderId = 2, ProductId = 6, Quantity = 10, UnitPrice = 58m,  ReceivedQuantity = 0,  CreatedAt = d },
            new PurchaseOrderItem { Id = 5, PurchaseOrderId = 2, ProductId = 9, Quantity = 20, UnitPrice = 22m,  ReceivedQuantity = 0,  CreatedAt = d }
        );

        mb.Entity<Recipe>().HasData(
            new Recipe { Id = 1, BusinessId = 1, Name = "Espresso",    Category = "Sıcak İçecek", SellingPrice = 45m,  IsActive = true, CreatedAt = d },
            new Recipe { Id = 2, BusinessId = 1, Name = "Latte",       Category = "Sıcak İçecek", SellingPrice = 65m,  IsActive = true, CreatedAt = d },
            new Recipe { Id = 3, BusinessId = 1, Name = "Cappuccino",  Category = "Sıcak İçecek", SellingPrice = 60m,  IsActive = true, CreatedAt = d },
            new Recipe { Id = 4, BusinessId = 1, Name = "Türk Çayı",   Category = "Sıcak İçecek", SellingPrice = 25m,  IsActive = true, CreatedAt = d },
            new Recipe { Id = 5, BusinessId = 1, Name = "Buzlu Latte", Category = "Soğuk İçecek", SellingPrice = 75m,  IsActive = true, CreatedAt = d }
        );

        // RecipeIngredient: Quantity birim cinsinden (kg, lt, adet, pk)
        mb.Entity<RecipeIngredient>().HasData(
            // Espresso: 0.018kg Arabica
            new RecipeIngredient { Id = 1,  RecipeId = 1, ProductId = 1,  Quantity = 0.018m, CreatedAt = d },
            // Latte: 0.018kg Arabica + 0.18lt Süt + 1 Bardak + 1 Kapak
            new RecipeIngredient { Id = 2,  RecipeId = 2, ProductId = 1,  Quantity = 0.018m, CreatedAt = d },
            new RecipeIngredient { Id = 3,  RecipeId = 2, ProductId = 3,  Quantity = 0.18m,  CreatedAt = d },
            new RecipeIngredient { Id = 4,  RecipeId = 2, ProductId = 7,  Quantity = 1m,     CreatedAt = d },
            new RecipeIngredient { Id = 5,  RecipeId = 2, ProductId = 8,  Quantity = 1m,     CreatedAt = d },
            // Cappuccino: 0.018kg Arabica + 0.12lt Süt + 1 Bardak + 1 Kapak
            new RecipeIngredient { Id = 6,  RecipeId = 3, ProductId = 1,  Quantity = 0.018m, CreatedAt = d },
            new RecipeIngredient { Id = 7,  RecipeId = 3, ProductId = 3,  Quantity = 0.12m,  CreatedAt = d },
            new RecipeIngredient { Id = 8,  RecipeId = 3, ProductId = 7,  Quantity = 1m,     CreatedAt = d },
            new RecipeIngredient { Id = 9,  RecipeId = 3, ProductId = 8,  Quantity = 1m,     CreatedAt = d },
            // Türk Çayı: 0.01pk Çay Poşeti + 1 Bardak
            new RecipeIngredient { Id = 10, RecipeId = 4, ProductId = 10, Quantity = 0.01m,  CreatedAt = d },
            new RecipeIngredient { Id = 11, RecipeId = 4, ProductId = 7,  Quantity = 1m,     CreatedAt = d },
            // Buzlu Latte: 0.018kg Arabica + 0.2lt Süt + 1 Bardak + 1 Kapak
            new RecipeIngredient { Id = 12, RecipeId = 5, ProductId = 1,  Quantity = 0.018m, CreatedAt = d },
            new RecipeIngredient { Id = 13, RecipeId = 5, ProductId = 3,  Quantity = 0.2m,   CreatedAt = d },
            new RecipeIngredient { Id = 14, RecipeId = 5, ProductId = 7,  Quantity = 1m,     CreatedAt = d },
            new RecipeIngredient { Id = 15, RecipeId = 5, ProductId = 8,  Quantity = 1m,     CreatedAt = d }
        );

        mb.Entity<StockTransaction>().HasData(
            new StockTransaction { Id = 1, BusinessId = 1, ProductId = 1, Type = StockTransactionType.Purchase, Quantity = 30, UnitCost = 485m, PurchaseOrderId = 1, TransactionDate = new DateTime(2025, 1, 12, 0, 0, 0, DateTimeKind.Utc), CreatedAt = d },
            new StockTransaction { Id = 2, BusinessId = 1, ProductId = 3, Type = StockTransactionType.Purchase, Quantity = 50, UnitCost = 42m,  PurchaseOrderId = 1, TransactionDate = new DateTime(2025, 1, 12, 0, 0, 0, DateTimeKind.Utc), CreatedAt = d }
        );
    }
}
