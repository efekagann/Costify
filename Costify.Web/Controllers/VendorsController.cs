using Costify.Domain.Entities;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Services;
using Costify.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Costify.Web.Controllers;

[Authorize]
public class VendorsController : Controller
{
    private readonly IVendorRepository _vendors;
    private readonly ICurrentBusinessService _currentBusiness;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public VendorsController(
        IVendorRepository vendors,
        ICurrentBusinessService currentBusiness,
        IStringLocalizer<SharedResource> localizer)
    {
        _vendors = vendors;
        _currentBusiness = currentBusiness;
        _localizer = localizer;
    }

    private const int PageSize = 25;

    public async Task<IActionResult> Index(int page = 1)
    {
        page = PaginatedList<object>.ClampPage(page);
        var query = _vendors.Query().OrderBy(v => v.Name);
        var paginated = await PaginatedList<Vendor>.CreateAsync(query, page, PageSize);
        return View(paginated);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vendor = await _vendors.GetWithOrdersAsync(id);
        if (vendor is null) return NotFound();
        return View(vendor);
    }

    public IActionResult Create() => View(new VendorViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VendorViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var vendor = new Vendor
        {
            BusinessId = _currentBusiness.BusinessId,
            Name = vm.Name,
            ContactPerson = vm.ContactPerson,
            Phone = vm.Phone,
            Email = vm.Email,
            Address = vm.Address,
            TaxNumber = vm.TaxNumber,
            IsActive = vm.IsActive
        };

        await _vendors.AddAsync(vendor);
        TempData["Success"] = string.Format(_localizer["Vendors_Added"].Value, vendor.Name);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vendor = await _vendors.GetByIdAsync(id);
        if (vendor is null) return NotFound();
        return View(VendorViewModel.FromEntity(vendor));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VendorViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var vendor = await _vendors.GetByIdAsync(id);
        if (vendor is null) return NotFound();

        vendor.Name = vm.Name;
        vendor.ContactPerson = vm.ContactPerson;
        vendor.Phone = vm.Phone;
        vendor.Email = vm.Email;
        vendor.Address = vm.Address;
        vendor.TaxNumber = vm.TaxNumber;
        vendor.IsActive = vm.IsActive;

        await _vendors.UpdateAsync(vendor);
        TempData["Success"] = string.Format(_localizer["Vendors_Updated"].Value, vendor.Name);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var vendor = await _vendors.GetByIdAsync(id);
        if (vendor is null) return NotFound();
        await _vendors.DeleteAsync(vendor);
        TempData["Success"] = _localizer["Vendors_Deleted"].Value;
        return RedirectToAction(nameof(Index));
    }
}
