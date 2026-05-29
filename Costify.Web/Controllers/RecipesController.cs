using Costify.Domain.Entities;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Costify.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Costify.Web.Controllers;

[Authorize]
public class RecipesController : Controller
{
    private readonly IRecipeRepository _recipes;
    private readonly CostifyDbContext _context;
    private readonly ICurrentBusinessService _currentBusiness;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RecipesController(
        IRecipeRepository recipes,
        CostifyDbContext context,
        ICurrentBusinessService currentBusiness,
        IStringLocalizer<SharedResource> localizer)
    {
        _recipes = recipes;
        _context = context;
        _currentBusiness = currentBusiness;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var recipes = await _recipes.GetAllWithIngredientsAsync();
        await PopulateViewBagAsync();
        return View(recipes);
    }

    public async Task<IActionResult> Details(int id)
    {
        var recipe = await _recipes.GetWithIngredientsAsync(id);
        if (recipe is null) return NotFound();
        return View(recipe);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Recipe recipe, int[] productIds, decimal[] quantities)
    {
        if (!ModelState.IsValid || productIds.Length == 0)
        {
            TempData["Error"] = _localizer["Recipes_AtLeastOneIngredient"].Value;
            return RedirectToAction(nameof(Index));
        }

        recipe.BusinessId = _currentBusiness.BusinessId;
        recipe.Ingredients = productIds.Select((pid, i) => new RecipeIngredient
        {
            ProductId = pid,
            Quantity = quantities[i]
        }).ToList();

        await _recipes.AddAsync(recipe);
        TempData["Success"] = string.Format(_localizer["Recipes_Created"].Value, recipe.Name);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Recipe recipe, int[] productIds, decimal[] quantities)
    {
        var existing = await _recipes.GetWithIngredientsAsync(id);
        if (existing is null) return NotFound();

        existing.Name = recipe.Name;
        existing.Category = recipe.Category;
        existing.Description = recipe.Description;
        existing.SellingPrice = recipe.SellingPrice;
        existing.IsActive = recipe.IsActive;

        _context.RecipeIngredients.RemoveRange(existing.Ingredients);
        existing.Ingredients = productIds.Select((pid, i) => new RecipeIngredient
        {
            ProductId = pid,
            Quantity = quantities[i]
        }).ToList();

        await _recipes.UpdateAsync(existing);
        TempData["Success"] = string.Format(_localizer["Recipes_Updated"].Value, existing.Name);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetIngredients(int id)
    {
        var recipe = await _recipes.GetWithIngredientsAsync(id);
        if (recipe is null) return NotFound();
        var result = recipe.Ingredients.Select(i => new { i.ProductId, i.Quantity });
        return Json(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var recipe = await _recipes.GetByIdAsync(id);
        if (recipe is null) return NotFound();
        await _recipes.DeleteAsync(recipe);
        TempData["Success"] = _localizer["Recipes_Deleted"].Value;
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateViewBagAsync()
    {
        var products = await _context.Products
            .Include(p => p.Unit)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
        ViewBag.Products = products;

        var categories = await _context.Recipes
            .Where(r => r.Category != null && r.Category != "")
            .Select(r => r.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        ViewBag.RecipeCategories = categories;
    }
}
