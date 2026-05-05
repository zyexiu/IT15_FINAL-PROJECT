using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SnackFlowMES.Models;
using SnackFlowMES.ViewModels;

namespace SnackFlowMES.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser>   _users;
    private readonly ILogger<AccountController>     _log;

    public AccountController(
        SignInManager<ApplicationUser> signIn,
        UserManager<ApplicationUser>   users,
        ILogger<AccountController>     log)
    {
        _signIn = signIn;
        _users  = users;
        _log    = log;
    }

    // ── GET /Account/Login ───────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Already logged in → go to dashboard
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // ── POST /Account/Login ──────────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Accept either email or username
        var user = await _users.FindByEmailAsync(model.Username)
                ?? await _users.FindByNameAsync(model.Username);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact the administrator.");
            return View(model);
        }

        var result = await _signIn.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _log.LogInformation("User {User} logged in.", user.UserName);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        if (result.IsLockedOut)
        {
            _log.LogWarning("User {User} account locked out.", user.UserName);
            ModelState.AddModelError(string.Empty, "Account locked. Try again in 15 minutes.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return View(model);
    }

    // ── POST /Account/Logout ─────────────────────────────────
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        _log.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    // ── GET /Account/AccessDenied ────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // ── POST /Account/LoginAjax ──────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAjax([FromBody] LoginViewModel model)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(model.Username))
            return Json(new { success = false, message = "Username is required" });

        if (string.IsNullOrWhiteSpace(model.Password))
            return Json(new { success = false, message = "Password is required" });

        // Accept either email or username
        var user = await _users.FindByEmailAsync(model.Username)
                ?? await _users.FindByNameAsync(model.Username);

        if (user is null)
        {
            _log.LogWarning("Failed login attempt for {Username}", model.Username);
            return Json(new { success = false, message = "Invalid username or password" });
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _log.LogWarning("Login attempt for inactive user {Username}", model.Username);
            return Json(new { success = false, message = "Your account is inactive. Please contact the administrator." });
        }

        var result = await _signIn.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _log.LogInformation("User {User} logged in via AJAX.", user.UserName);
            return Json(new { success = true, redirectUrl = "/Dashboard" });
        }

        if (result.IsLockedOut)
        {
            _log.LogWarning("User {User} account locked out.", user.UserName);
            return Json(new { success = false, message = "Account locked. Try again in 15 minutes." });
        }

        return Json(new { success = false, message = "Invalid username or password" });
    }
}
