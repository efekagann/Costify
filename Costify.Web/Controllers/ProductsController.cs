using Costify.Domain.Entities;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Costify.Infrastructure.Services;
using Costify.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Costify.Web.Controllers;

[Authorize]

public class ProductsController : Controller
{
    private readonly IProductRepository _products;
    private readonly CostifyDbContext _context;
    private readonly ICurrentBusinessService _currentBusiness;

    public ProductsController(
        IProductRepository products,
        CostifyDbContext context,
        ICurrentBusinessService currentBusiness)
    {
        _products = products;
        _context = context;
        _currentBusiness = currentBusiness;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        var query = _products.Query()
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.SKU != null && p.SKU.Contains(search)));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        var categories = await _context.Categories.ToListAsync();

        var allUnits = await _context.Units.ToListAsync();

        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
        ViewBag.AllCategories = categories;
        ViewBag.AllUnits = allUnits;

        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _products.GetWithDetailsAsync(id);
        if (product is null) return NotFound();
        return View(product);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new ProductViewModel();
        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var product = new Product
        {
            BusinessId = _currentBusiness.BusinessId,
            Name = vm.Name,
            Description = vm.Description,
            SKU = vm.SKU,
            CategoryId = vm.CategoryId,
            UnitId = vm.UnitId,
            CurrentStock = vm.CurrentStock,
            MinimumStock = vm.MinimumStock,
            LastPurchasePrice = vm.LastPurchasePrice,
            IsActive = vm.IsActive
        };

        await _products.AddAsync(product);
        TempData["Success"] = $"'{product.Name}' ürünü başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product is null) return NotFound();

        var vm = ProductViewModel.FromEntity(product);
        await PopulateDropdownsAsync(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(vm);
            return View(vm);
        }

        var product = await _products.GetByIdAsync(id);
        if (product is null) return NotFound();

        product.Name = vm.Name;
        product.Description = vm.Description;
        product.SKU = vm.SKU;
        product.CategoryId = vm.CategoryId;
        product.UnitId = vm.UnitId;
        product.MinimumStock = vm.MinimumStock;
        product.LastPurchasePrice = vm.LastPurchasePrice;
        product.IsActive = vm.IsActive;

        await _products.UpdateAsync(product);
        TempData["Success"] = $"'{product.Name}' güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product is null) return NotFound();

        await _products.DeleteAsync(product);
        TempData["Success"] = $"'{product.Name}' silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(ProductViewModel vm)
    {
        var categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
        var units = await _context.Units.Where(u => !u.IsDeleted).ToListAsync();
        vm.Categories = new SelectList(categories, "Id", "Name", vm.CategoryId);
        vm.Units = new SelectList(units, "Id", "Name", vm.UnitId);
    }
}
