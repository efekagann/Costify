using Costify.Infrastructure.Data;
using Costify.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Costify.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly CostifyDbContext _context;
    private readonly ICurrentBusinessService _currentBusiness;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SettingsController(
        CostifyDbContext context,
        ICurrentBusinessService currentBusiness,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _currentBusiness = currentBusiness;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var business = await _context.Businesses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == _currentBusiness.BusinessId);

        if (business is null) return NotFound();
        return View(business);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        string name, string? taxNumber, string? phone, string? email,
        string? address, string? logoUrl)
    {
        var business = await _context.Businesses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Id == _currentBusiness.BusinessId);

        if (business is null) return NotFound();

        business.Name = name;
        business.TaxNumber = taxNumber;
        business.Phone = phone;
        business.Email = email;
        business.Address = address;
        business.LogoUrl = logoUrl;

        await _context.SaveChangesAsync();
        TempData["Success"] = _localizer["Settings_Updated"].Value;
        return RedirectToAction(nameof(Index));
    }
}
