using Costify.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Costify.Web.Controllers;

public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IStringLocalizer<SharedResource> localizer)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _localizer = localizer;
    }

    // ── Login ──────────────────────────────────────────────────────────────

    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (_signInManager.IsSignedIn(User))
            return RedirectToAction("Index", "Dashboard");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl)
    {
        var result = await _signInManager.PasswordSignInAsync(
            username, password,
            isPersistent: true,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            ViewBag.Error = result.IsLockedOut
                ? _localizer["Auth_LockedOut"].Value
                : _localizer["Auth_InvalidCredentials"].Value;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // ── Register ───────────────────────────────────────────────────────────

    [AllowAnonymous, HttpGet]
    public IActionResult Register()
    {
        if (_signInManager.IsSignedIn(User))
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string displayName, string username, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ViewBag.Error = _localizer["Auth_PasswordMismatch"].Value;
            return View();
        }

        if (await _userManager.FindByNameAsync(username) is not null)
        {
            ViewBag.Error = _localizer["Auth_UsernameTaken"].Value;
            return View();
        }

        var user = new ApplicationUser
        {
            UserName = username,
            DisplayName = displayName,
            EmailConfirmed = true,
            BusinessId = 1
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            ViewBag.Error = string.Join(" ", result.Errors.Select(e => e.Description));
            return View();
        }

        await _signInManager.SignInAsync(user, isPersistent: true);
        TempData["Success"] = _localizer["Auth_RegisterSuccess"].Value;
        return RedirectToAction("Index", "Dashboard");
    }

    // ── Profile ────────────────────────────────────────────────────────────

    [Authorize, HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        ViewBag.DisplayName = user.DisplayName;
        ViewBag.Username = user.UserName;
        ViewBag.Email = user.Email;
        return View();
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(string displayName, string? email)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        user.DisplayName = displayName;
        if (!string.IsNullOrWhiteSpace(email))
            user.Email = email;

        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = _localizer["Auth_ProfileUpdated"].Value;
        return RedirectToAction(nameof(Profile));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        string currentPassword, string newPassword, string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            TempData["Error"] = _localizer["Auth_PasswordMismatch"].Value;
            return RedirectToAction(nameof(Profile));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var isWrongCurrent = result.Errors.Any(e => e.Code == "PasswordMismatch");
            TempData["Error"] = isWrongCurrent
                ? _localizer["Auth_WrongCurrentPassword"].Value
                : _localizer["Auth_PasswordChangeFailed"].Value;
            return RedirectToAction(nameof(Profile));
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Success"] = _localizer["Auth_PasswordChanged"].Value;
        return RedirectToAction(nameof(Profile));
    }
}
