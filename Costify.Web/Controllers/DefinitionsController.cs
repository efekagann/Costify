using Costify.Domain.Entities;
using Costify.Infrastructure.Data;
using Costify.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Costify.Web.Controllers;

[Authorize]
public class DefinitionsController : Controller
{
    private readonly CostifyDbContext _context;
    private readonly ICurrentBusinessService _currentBusiness;

    public DefinitionsController(CostifyDbContext context, ICurrentBusinessService currentBusiness)
    {
        _context = context;
        _currentBusiness = currentBusiness;
    }

    public async Task<IActionResult> Index(string tab = "categories")
    {
        ViewBag.ActiveTab = tab;
        ViewBag.Categories = await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.Name).ToListAsync();
        ViewBag.Units = await _context.Units
            .Include(u => u.Products)
            .OrderBy(u => u.Name).ToListAsync();
        return View();
    }

    // ── Kategori ─────────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name, string? description, string? colorHex)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Kategori adı zorunludur.";
            return RedirectToAction(nameof(Index), new { tab = "categories" });
        }

        _context.Categories.Add(new Category
        {
            BusinessId = _currentBusiness.BusinessId,
            Name = name.Trim(),
            Description = description?.Trim(),
            ColorHex = string.IsNullOrEmpty(colorHex) ? "#6b7280" : colorHex
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = $"'{name}' kategorisi eklendi.";
        return RedirectToAction(nameof(Index), new { tab = "categories" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, string name, string? description, string? colorHex)
    {
        var cat = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (cat is null) return NotFound();

        cat.Name = name.Trim();
        cat.Description = description?.Trim();
        cat.ColorHex = string.IsNullOrEmpty(colorHex) ? cat.ColorHex : colorHex;
        cat.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"'{cat.Name}' güncellendi.";
        return RedirectToAction(nameof(Index), new { tab = "categories" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var inUse = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (inUse)
        {
            TempData["Error"] = "Bu kategoride ürünler var, önce ürünleri başka kategoriye taşıyın.";
            return RedirectToAction(nameof(Index), new { tab = "categories" });
        }

        var cat = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (cat is null) return NotFound();
        cat.IsDeleted = true;
        cat.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Kategori silindi.";
        return RedirectToAction(nameof(Index), new { tab = "categories" });
    }

    // ── Birim ────────────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUnit(string name, string symbol)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(symbol))
        {
            TempData["Error"] = "Ad ve sembol zorunludur.";
            return RedirectToAction(nameof(Index), new { tab = "units" });
        }

        _context.Units.Add(new Unit
        {
            Name = name.Trim(),
            Symbol = symbol.Trim()
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = $"'{name}' birimi eklendi.";
        return RedirectToAction(nameof(Index), new { tab = "units" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUnit(int id, string name, string symbol)
    {
        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id);
        if (unit is null) return NotFound();

        unit.Name = name.Trim();
        unit.Symbol = symbol.Trim();
        unit.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"'{unit.Name}' birimi güncellendi.";
        return RedirectToAction(nameof(Index), new { tab = "units" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        var inUse = await _context.Products.AnyAsync(p => p.UnitId == id);
        if (inUse)
        {
            TempData["Error"] = "Bu birim ürünlerde kullanılıyor, silinemez.";
            return RedirectToAction(nameof(Index), new { tab = "units" });
        }

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id);
        if (unit is null) return NotFound();
        unit.IsDeleted = true;
        unit.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Birim silindi.";
        return RedirectToAction(nameof(Index), new { tab = "units" });
    }
}
