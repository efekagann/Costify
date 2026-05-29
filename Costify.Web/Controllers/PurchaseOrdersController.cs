using Costify.Domain.Entities;
using Costify.Domain.Enums;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Costify.Infrastructure.Services;
using Costify.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Costify.Web.Controllers;

[Authorize]
public class PurchaseOrdersController : Controller
{
    private readonly IPurchaseOrderRepository _orders;
    private readonly IProductRepository _products;
    private readonly IVendorRepository _vendors;
    private readonly CostifyDbContext _context;
    private readonly ICurrentBusinessService _currentBusiness;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public PurchaseOrdersController(
        IPurchaseOrderRepository orders,
        IProductRepository products,
        IVendorRepository vendors,
        CostifyDbContext context,
        ICurrentBusinessService currentBusiness,
        IStringLocalizer<SharedResource> localizer)
    {
        _orders = orders;
        _products = products;
        _vendors = vendors;
        _context = context;
        _currentBusiness = currentBusiness;
        _localizer = localizer;
    }

    private const int PageSize = 20;

    public async Task<IActionResult> Index(int page = 1)
    {
        page = PaginatedList<object>.ClampPage(page);

        var query = _context.PurchaseOrders
            .Include(o => o.Vendor)
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate);

        var paginated = await PaginatedList<PurchaseOrder>.CreateAsync(query, page, PageSize);
        await PopulateViewBagAsync();
        return View(paginated);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orders.GetWithItemsAsync(id);
        if (order is null) return NotFound();

        if (order.Status == OrderStatus.Ordered || order.Status == OrderStatus.Draft)
            await PopulateViewBagAsync();

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
                ModelState.AddModelError("", _localizer["PO_AtLeastOneProduct"].Value);
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
        TempData["Success"] = string.Format(_localizer["PO_Created"].Value, order.OrderNumber);
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(int id)
    {
        var order = await _orders.GetWithItemsAsync(id);
        if (order is null) return NotFound();
        if (order.Status == OrderStatus.Received)
        {
            TempData["Error"] = _localizer["PO_AlreadyReceived"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = OrderStatus.Received;
        order.DeliveryDate = DateTime.UtcNow;

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

        TempData["Success"] = _localizer["PO_ReceivedSuccess"].Value;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PurchaseOrder order,
        int[] productIds,
        decimal[] quantities,
        decimal[] unitPrices)
    {
        var existing = await _orders.GetWithItemsAsync(id);
        if (existing is null) return NotFound();

        if (existing.Status != OrderStatus.Ordered && existing.Status != OrderStatus.Draft)
        {
            TempData["Error"] = _localizer["PO_CannotEdit"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }

        if (productIds.Length == 0)
        {
            TempData["Error"] = _localizer["PO_AtLeastOneProduct"].Value;
            await PopulateViewBagAsync();
            return View(existing);
        }

        existing.VendorId = order.VendorId;
        existing.OrderDate = order.OrderDate;
        existing.Notes = order.Notes;

        _context.PurchaseOrderItems.RemoveRange(existing.Items);
        existing.Items = productIds.Select((pid, i) => new PurchaseOrderItem
        {
            ProductId = pid,
            Quantity = quantities[i],
            UnitPrice = unitPrices[i]
        }).ToList();
        existing.TotalAmount = existing.Items.Sum(x => x.Quantity * x.UnitPrice);

        await _orders.UpdateAsync(existing);
        await _context.SaveChangesAsync();

        TempData["Success"] = string.Format(_localizer["PO_Updated"].Value, existing.OrderNumber);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _orders.GetByIdAsync(id);
        if (order is null) return NotFound();

        if (order.Status == OrderStatus.Received || order.Status == OrderStatus.Cancelled)
        {
            TempData["Error"] = _localizer["PO_CannotCancel"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = OrderStatus.Cancelled;
        await _orders.UpdateAsync(order);
        TempData["Success"] = _localizer["PO_Cancelled"].Value;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PartialReceive(int id, int[] itemIds, decimal[] receivedQtys)
    {
        var order = await _orders.GetWithItemsAsync(id);
        if (order is null) return NotFound();

        if (order.Status == OrderStatus.Received || order.Status == OrderStatus.Cancelled)
        {
            TempData["Error"] = _localizer["PO_AlreadyReceived"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }

        for (int i = 0; i < itemIds.Length; i++)
        {
            var item = order.Items.FirstOrDefault(x => x.Id == itemIds[i]);
            if (item is null || receivedQtys[i] <= 0) continue;

            var delta = receivedQtys[i] - item.ReceivedQuantity;
            if (delta <= 0) continue;

            item.ReceivedQuantity = receivedQtys[i];

            var product = await _products.GetByIdAsync(item.ProductId);
            if (product is null) continue;

            product.CurrentStock += delta;
            product.LastPurchasePrice = item.UnitPrice;
            await _products.UpdateAsync(product);

            _context.StockTransactions.Add(new StockTransaction
            {
                BusinessId = _currentBusiness.BusinessId,
                ProductId = item.ProductId,
                Type = StockTransactionType.Purchase,
                Quantity = delta,
                UnitCost = item.UnitPrice,
                PurchaseOrderId = order.Id,
                TransactionDate = DateTime.UtcNow
            });
        }

        var allReceived = order.Items.All(x => x.ReceivedQuantity >= x.Quantity);
        order.Status = allReceived ? OrderStatus.Received : OrderStatus.PartiallyReceived;
        if (allReceived) order.DeliveryDate = DateTime.UtcNow;

        await _orders.UpdateAsync(order);
        await _context.SaveChangesAsync();

        TempData["Success"] = allReceived
            ? _localizer["PO_ReceivedSuccess"].Value
            : _localizer["PO_PartialReceived"].Value;
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
