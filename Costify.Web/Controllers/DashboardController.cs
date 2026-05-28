using Costify.Domain.Enums;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Costify.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Costify.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IProductRepository _products;
    private readonly IVendorRepository _vendors;
    private readonly IPurchaseOrderRepository _orders;
    private readonly CostifyDbContext _context;

    public DashboardController(
        IProductRepository products,
        IVendorRepository vendors,
        IPurchaseOrderRepository orders,
        CostifyDbContext context)
    {
        _products = products;
        _vendors = vendors;
        _orders = orders;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var allProducts = await _products.GetAllAsync();
        var lowStock = await _products.GetLowStockProductsAsync();
        var allVendors = await _vendors.GetAllAsync();
        var recentOrders = await _orders.GetRecentOrdersAsync(5);

        var thisMonth = DateTime.UtcNow;
        var monthlySpend = await _context.PurchaseOrders
            .Where(o => o.CreatedAt.Year == thisMonth.Year
                     && o.CreatedAt.Month == thisMonth.Month
                     && o.Status == OrderStatus.Received)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var pendingOrders = await _context.PurchaseOrders
            .CountAsync(o => o.Status == OrderStatus.Ordered || o.Status == OrderStatus.PartiallyReceived);

        var vm = new DashboardViewModel
        {
            TotalProducts = allProducts.Count,
            LowStockCount = lowStock.Count,
            TotalVendors = allVendors.Count,
            PendingOrdersCount = pendingOrders,
            MonthlySpend = monthlySpend,
            LowStockProducts = lowStock.Take(5).ToList(),
            RecentOrders = recentOrders.ToList()
        };

        return View(vm);
    }
}
