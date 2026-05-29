using Costify.Infrastructure;
using Costify.Infrastructure.Data;
using Costify.Web.Data;
using Costify.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

// ── Localization ──────────────────────────────────────────────────────────
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// ── Infrastructure (business data context + repositories) ────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Identity (auth context — separate from business context) ─────────────
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 1;
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<AppIdentityDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.Name = "Costify.Auth";
});

var app = builder.Build();

// ── Database migrations & seed ────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    // Business data migration
    var db = scope.ServiceProvider.GetRequiredService<CostifyDbContext>();
    await db.Database.MigrateAsync();

    // Identity migration
    var identityDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    await identityDb.Database.MigrateAsync();

    // Seed admin user & role from config
    await SeedIdentityAsync(scope.ServiceProvider, app.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var supportedCultures = new[] { new CultureInfo("tr-TR"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

// ── Identity seed ─────────────────────────────────────────────────────────
static async Task SeedIdentityAsync(IServiceProvider services, IConfiguration config)
{
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    // Create Admin role
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    var username = config["AdminCredentials:Username"] ?? "admin";
    var password = config["AdminCredentials:Password"] ?? "";
    var email = config["AdminCredentials:Email"] ?? $"{username}@costify.app";
    var displayName = config["AdminCredentials:DisplayName"] ?? "Yönetici";

    if (string.IsNullOrEmpty(password))
    {
        logger.LogWarning("⚠️  AdminCredentials:Password is not configured. Admin user will not be seeded.");
        return;
    }

    // Mevcut kullanıcılarda BusinessId=0 olanları düzelt (migration default 0 koyuyor)
    foreach (var u in userManager.Users.Where(u => u.BusinessId == 0).ToList())
    {
        u.BusinessId = 1;
        await userManager.UpdateAsync(u);
    }

    if (await userManager.FindByNameAsync(username) is null)
    {
        var admin = new ApplicationUser
        {
            UserName = username,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true,
            BusinessId = 1
        };

        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            logger.LogInformation("✅ Admin user '{Username}' seeded successfully.", username);
            logger.LogWarning("⚠️  Remove AdminCredentials:Password from appsettings.json and use user-secrets instead:\n" +
                              "    dotnet user-secrets set \"AdminCredentials:Password\" \"{password}\"", password);
        }
        else
        {
            logger.LogError("❌ Failed to seed admin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
