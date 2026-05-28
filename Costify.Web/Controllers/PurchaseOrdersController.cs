using Costify.Domain.Entities;
using Costify.Domain.Enums;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Costify.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Costify.Web.Controllers;

[Authorize]

public class PurchaseOrdersController : Controller
{
    private readonly IPurchaseOrderRepository _orders;
    private readonly IProductRepository _products;
    private readonly IVendorRepository _vendors;
    private readonly CostifyDbContext _context;
    private readonly ICurrentBusinessService _currentBusiness;

    public PurchaseOrdersController(
        IPurchaseOrderRepository orders,
        IProductRepository products,
        IVendorRepository vendors,
        CostifyDbContext context,
        ICurrentBusinessService currentBusiness)
    {
        _orders = orders;
        _products = products;
        _vendors = vendors;
        _context = context;
        _currentBusiness = currentBusiness;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _context.PurchaseOrders
            .Include(o => o.Vendor)
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
        await PopulateViewBagAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orders.GetWithItemsAsync(id);
        if (order is null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateViewBagAsync();
        return View(new PurchaseOrder { OrderDate = DateTime.Today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PurchaseOrder order,
        int[] productIds,
        decimal[] quantities,
        decimal[] unitPrices)
    {
        if (!ModelState.IsValid || productIds.Length == 0)
        {
            if (productIds.Length == 0)
                ModelState.AddModelError("", "En az bir ürün eklemelisiniz.");
            await PopulateViewBagAsync();
            return View(order);
        }

        order.BusinessId = _currentBusiness.BusinessId;
        order.OrderNumber = await _orders.GenerateOrderNumberAsync();
        order.Status = OrderStatus.Ordered;

        var items = productIds.Select((pid, i) => new PurchaseOrderItem
        {
            ProductId = pid,
            Quantity = quantities[i],
            UnitPrice = unitPrices[i]
        }).ToList();

        order.Items = items;
        order.TotalAmount = items.Sum(x => x.Quantity * x.UnitPrice);

        await _orders.AddAsync(order);
        TempData["Success"] = $"Sipariş {order.OrderNumber} oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(int id)
    {
        var order = await _orders.GetWithItemsAsync(id);
        if (order is null) return NotFound();
        if (order.Status == OrderStatus.Received)
        {
            TempData["Error"] = "Bu sipariş zaten teslim alındı.";
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = OrderStatus.Received;
        order.DeliveryDate = DateTime.UtcNow;

        // Stok hareketi oluştur ve ürün stoğunu güncelle
        foreach (var item in order.Items)
        {
            var product = await _products.GetByIdAsync(item.ProductId);
            if (product is null) continue;

            product.CurrentStock += item.Quantity;
            product.LastPurchasePrice = item.UnitPrice;
            await _products.UpdateAsync(product);

            _context.StockTransactions.Add(new StockTransaction
            {
                BusinessId = _currentBusiness.BusinessId,
                ProductId = item.ProductId,
                Type = StockTransactionType.Purchase,
                Quantity = item.Quantity,
                UnitCost = item.UnitPrice,
                PurchaseOrderId = order.Id,
                TransactionDate = DateTime.UtcNow
            });
        }

        await _orders.UpdateAsync(order);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Sipariş teslim alındı, stoklar güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateViewBagAsync()
    {
        var vendors = await _vendors.FindAsync(v => v.IsActive);
        var products = await _context.Products
            .Include(p => p.Unit)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        ViewBag.Vendors = new SelectList(vendors, "Id", "Name");
        ViewBag.Products = products;
    }
}
