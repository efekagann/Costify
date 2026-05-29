using Costify.Domain.Entities;
using Costify.Domain.Enums;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Costify.Infrastructure.Services;
using Costify.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Costify.Web.Controllers;

[Authorize]
public class StockCountsController : Controller
{
    private readonly IStockCountRepository _counts;
    private readonly IProductRepository _products;
    private readonly CostifyDbContext _context;
    private readonly ICurrentBusinessService _currentBusiness;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public StockCountsController(
        IStockCountRepository counts,
        IProductRepository products,
        CostifyDbContext context,
        ICurrentBusinessService currentBusiness,
        IStringLocalizer<SharedResource> localizer)
    {
        _counts = counts;
        _products = products;
        _context = context;
        _currentBusiness = currentBusiness;
        _localizer = localizer;
    }

    private const int PageSize = 15;

    public async Task<IActionResult> Index(int page = 1)
    {
        page = PaginatedList<object>.ClampPage(page);

        var query = _context.StockCounts
            .Include(sc => sc.Items)
            .OrderByDescending(sc => sc.CountDate);

        var paginated = await PaginatedList<StockCount>.CreateAsync(query, page, PageSize);
        return View(paginated);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string? notes)
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        var count = new StockCount
        {
            BusinessId = _currentBusiness.BusinessId,
            CountDate = DateTime.UtcNow,
            Notes = notes,
            Status = StockCountStatus.InProgress,
            Items = products.Select(p => new StockCountItem
            {
                ProductId = p.Id,
                TheoreticalQuantity = p.CurrentStock,
                CountedQuantity = p.CurrentStock
            }).ToList()
        };

        await _counts.AddAsync(count);
        TempData["Success"] = _localizer["SC_Created"].Value;
        return RedirectToAction(nameof(Details), new { id = count.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var count = await _counts.GetWithItemsAsync(id);
        if (count is null) return NotFound();
        return View(count);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCounts(int id, int[] itemIds, decimal[] countedQtys)
    {
        var count = await _counts.GetWithItemsAsync(id);
        if (count is null) return NotFound();
        if (count.Status == StockCountStatus.Completed)
        {
            TempData["Error"] = _localizer["SC_CompletedCannotEdit"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }

        for (int i = 0; i < itemIds.Length; i++)
        {
            var item = count.Items.FirstOrDefault(x => x.Id == itemIds[i]);
            if (item is not null) item.CountedQuantity = countedQtys[i];
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = _localizer["SC_ValuesSaved"].Value;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var count = await _counts.GetByIdAsync(id);
        if (count is null) return NotFound();

        if (count.Status == StockCountStatus.Completed)
        {
            TempData["Error"] = _localizer["SC_CannotDelete"].Value;
            return RedirectToAction(nameof(Index));
        }

        await _counts.DeleteAsync(count);
        TempData["Success"] = _localizer["SC_Deleted"].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalize(int id)
    {
        var count = await _counts.GetWithItemsAsync(id);
        if (count is null) return NotFound();
        if (count.Status == StockCountStatus.Completed)
        {
            TempData["Error"] = _localizer["SC_AlreadyCompleted"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }

        foreach (var item in count.Items.Where(i => i.Difference != 0))
        {
            var product = await _products.GetByIdAsync(item.ProductId);
            if (product is null) continue;

            product.CurrentStock = item.CountedQuantity;
            await _products.UpdateAsync(product);

            _context.StockTransactions.Add(new StockTransaction
            {
                BusinessId = _currentBusiness.BusinessId,
                ProductId = item.ProductId,
                Type = StockTransactionType.Adjustment,
                Quantity = item.Difference,
                UnitCost = product.LastPurchasePrice,
                Notes = $"Stok sayımı #{count.Id}",
                TransactionDate = DateTime.UtcNow
            });
        }

        count.Status = StockCountStatus.Completed;
        await _counts.UpdateAsync(count);
        await _context.SaveChangesAsync();

        TempData["Success"] = _localizer["SC_Finalized"].Value;
        return RedirectToAction(nameof(Details), new { id });
    }
}
