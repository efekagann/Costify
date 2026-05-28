using Costify.Domain.Entities;

namespace Costify.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int TotalVendors { get; set; }
    public int PendingOrdersCount { get; set; }
    public decimal MonthlySpend { get; set; }
    public IList<Product> LowStockProducts { get; set; } = new List<Product>();
    public IList<PurchaseOrder> RecentOrders { get; set; } = new List<PurchaseOrder>();
}
