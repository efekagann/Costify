using Costify.Domain.Enums;
using Costify.Infrastructure.Data;
using Costify.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Costify.Web.Controllers;

[Authorize]
public class StockTransactionsController : Controller
{
    private readonly CostifyDbContext _context;

    public StockTransactionsController(CostifyDbContext context)
    {
        _context = context;
    }

    private const int PageSize = 30;

    public async Task<IActionResult> Index(
        int? productId, StockTransactionType? type,
        DateTime? from, DateTime? to, int page = 1)
    {
        page = PaginatedList<object>.ClampPage(page);

        var query = _context.StockTransactions
            .Include(t => t.Product).ThenInclude(p => p.Unit)
            .Include(t => t.PurchaseOrder)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(t => t.ProductId == productId.Value);
        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);
        if (from.HasValue)
            query = query.Where(t => t.TransactionDate >= from.Value.ToUniversalTime());
        if (to.HasValue)
            query = query.Where(t => t.TransactionDate <= to.Value.ToUniversalTime().AddDays(1));

        query = query.OrderByDescending(t => t.TransactionDate);

        var paginated = await PaginatedList<Costify.Domain.Entities.StockTransaction>
            .CreateAsync(query, page, PageSize);

        var products = await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name })
            .ToListAsync();

        ViewBag.Products = products;
        ViewBag.ProductId = productId;
        ViewBag.Type = type;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");

        return View(paginated);
    }
}
