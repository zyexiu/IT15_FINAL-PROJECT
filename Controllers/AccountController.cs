using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SnackFlowMES.Models;
using SnackFlowMES.ViewModels;
using SnackFlowMES.Services;

namespace SnackFlowMES.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser>   _users;
    private readonly IEmailService                  _email;
    private readonly IRecaptchaService              _recaptcha;
    private readonly IConfiguration                 _config;
    private readonly ILogger<AccountController>     _log;

    public AccountController(
        SignInManager<ApplicationUser> signIn,
        UserManager<ApplicationUser>   users,
        IEmailService                  email,
        IRecaptchaService              recaptcha,
        IConfiguration                 config,
        ILogger<AccountController>     log)
    {
        _signIn = signIn;
        _users  = users;
        _email  = email;
        _recaptcha = recaptcha;
        _config = config;
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
        // Verify reCAPTCHA token
        if (string.IsNullOrWhiteSpace(model.RecaptchaToken))
        {
            _log.LogWarning("Login attempt without reCAPTCHA token - allowing for testing");
            // Temporarily allow login without reCAPTCHA for testing
            // TODO: Remove this bypass after reCAPTCHA is confirmed working
        }
        else
        {
            var minimumScore = _config.GetValue<double>("RecaptchaSettings:MinimumScore", 0.5);
            var isValidRecaptcha = await _recaptcha.IsValidAsync(model.RecaptchaToken, "login", minimumScore);
            
            if (!isValidRecaptcha)
            {
                _log.LogWarning("Login attempt failed reCAPTCHA verification for {Username}", model.Username);
                return Json(new { success = false, message = "Security verification failed. Please try again." });
            }
        }

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

    // ── POST /Account/RegisterAjax ───────────────────────────
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAjax([FromBody] RegisterViewModel model)
    {
        try
        {
            // Verify reCAPTCHA token
            if (string.IsNullOrWhiteSpace(model.RecaptchaToken))
            {
                _log.LogWarning("Registration attempt without reCAPTCHA token - allowing for testing");
                // Temporarily allow registration without reCAPTCHA for testing
                // TODO: Remove this bypass after reCAPTCHA is confirmed working
            }
            else
            {
                var minimumScore = _config.GetValue<double>("RecaptchaSettings:MinimumScore", 0.5);
                var isValidRecaptcha = await _recaptcha.IsValidAsync(model.RecaptchaToken, "signup", minimumScore);
                
                if (!isValidRecaptcha)
                {
                    _log.LogWarning("Registration attempt failed reCAPTCHA verification for {Email}", model.Email);
                    return Json(new { success = false, message = "Security verification failed. Please try again." });
                }
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(model.FullName))
                return Json(new { success = false, message = "Full name is required" });

            if (string.IsNullOrWhiteSpace(model.Email))
                return Json(new { success = false, message = "Email is required" });

            if (string.IsNullOrWhiteSpace(model.UserName))
                return Json(new { success = false, message = "Username is required" });

            if (string.IsNullOrWhiteSpace(model.Password))
                return Json(new { success = false, message = "Password is required" });

            if (model.Password != model.ConfirmPassword)
                return Json(new { success = false, message = "Passwords do not match" });

            if (!model.AcceptTerms)
                return Json(new { success = false, message = "You must accept the terms and conditions" });

            // Validate email format
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return Json(new { success = false, message = "Invalid email address format" });

            // Validate username format (alphanumeric, hyphens, underscores only)
            if (!System.Text.RegularExpressions.Regex.IsMatch(model.UserName, @"^[a-zA-Z0-9_-]+$"))
                return Json(new { success = false, message = "Username can only contain letters, numbers, hyphens, and underscores" });

            // Check if username already exists
            var existingUser = await _users.FindByNameAsync(model.UserName);
            if (existingUser != null)
            {
                _log.LogWarning("Registration attempt with existing username: {Username}", model.UserName);
                return Json(new { success = false, message = "Username is already taken" });
            }

            // Check if email already exists
            var existingEmail = await _users.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                _log.LogWarning("Registration attempt with existing email: {Email}", model.Email);
                return Json(new { success = false, message = "Email is already registered" });
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Create user with password
            var result = await _users.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _log.LogWarning("User creation failed for {Username}: {Errors}", model.UserName, errors);
                
                // Return first error message
                var firstError = result.Errors.FirstOrDefault()?.Description ?? "Registration failed";
                return Json(new { success = false, message = firstError });
            }

            // Set TenantId to user's own ID (Admin becomes a tenant)
            user.TenantId = user.Id;
            await _users.UpdateAsync(user);

            // Assign Admin role (since sign up is only for Admin)
            var roleResult = await _users.AddToRoleAsync(user, "Admin");
            if (!roleResult.Succeeded)
            {
                _log.LogError("Failed to assign Admin role to user {Username}", model.UserName);
                // Delete the user if role assignment fails
                await _users.DeleteAsync(user);
                return Json(new { success = false, message = "Failed to assign admin role. Please try again." });
            }

            _log.LogInformation("New Admin user registered: {Username} ({Email}) with TenantId: {TenantId}", user.UserName, user.Email, user.TenantId);

            // Auto sign-in the new user
            await _signIn.SignInAsync(user, isPersistent: false);

            // Send welcome email (fire and forget - don't block registration)
            _ = Task.Run(async () =>
            {
                try
                {
                    var emailSent = await _email.SendWelcomeEmailAsync(user.Email!, user.FullName, user.UserName!);
                    if (emailSent)
                    {
                        _log.LogInformation("Welcome email sent to {Email}", user.Email);
                    }
                    else
                    {
                        _log.LogWarning("Failed to send welcome email to {Email}", user.Email);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error sending welcome email to {Email}", user.Email);
                }
            });

            return Json(new 
            { 
                success = true, 
                message = "Account created successfully! Redirecting to dashboard...",
                redirectUrl = "/Dashboard" 
            });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error during user registration");
            return Json(new { success = false, message = "An error occurred during registration. Please try again." });
        }
    }

    // ── GET /Account/Profile ─────────────────────────────────
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var roles = await _users.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var model = new ProfileViewModel
        {
            FullName = user.FullName,
            UserName = user.UserName!,
            Email = user.Email!,
            Role = role,
            CreatedAt = user.CreatedAt,
            CurrentUserName = user.UserName!,
            CurrentEmail = user.Email!
        };

        return View(model);
    }

    // ── POST /Account/Profile ────────────────────────────────
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var user = await _users.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _users.GetRolesAsync(user);
                model.Role = roles.FirstOrDefault() ?? "User";
                model.CreatedAt = user.CreatedAt;
            }
            return View(model);
        }

        var currentUser = await _users.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login");
        }

        // Check if username changed and is unique
        if (model.UserName != currentUser.UserName)
        {
            var existingUser = await _users.FindByNameAsync(model.UserName);
            if (existingUser != null)
            {
                ModelState.AddModelError("UserName", "Username is already taken");
                var roles = await _users.GetRolesAsync(currentUser);
                model.Role = roles.FirstOrDefault() ?? "User";
                model.CreatedAt = currentUser.CreatedAt;
                return View(model);
            }
        }

        // Check if email changed and is unique
        if (model.Email != currentUser.Email)
        {
            var existingEmail = await _users.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email is already registered");
                var roles = await _users.GetRolesAsync(currentUser);
                model.Role = roles.FirstOrDefault() ?? "User";
                model.CreatedAt = currentUser.CreatedAt;
                return View(model);
            }
        }

        // Update user properties
        currentUser.FullName = model.FullName;
        currentUser.UserName = model.UserName;
        currentUser.Email = model.Email;
        currentUser.NormalizedUserName = model.UserName.ToUpper();
        currentUser.NormalizedEmail = model.Email.ToUpper();

        var result = await _users.UpdateAsync(currentUser);

        if (result.Succeeded)
        {
            _log.LogInformation("User {User} updated profile successfully", currentUser.UserName);
            TempData["Success"] = "Profile updated successfully!";
            
            // Re-sign in to update claims
            await _signIn.RefreshSignInAsync(currentUser);
            
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        var userRoles = await _users.GetRolesAsync(currentUser);
        model.Role = userRoles.FirstOrDefault() ?? "User";
        model.CreatedAt = currentUser.CreatedAt;
        return View(model);
    }

    // ── GET /Account/ChangePassword ──────────────────────────
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    // ── POST /Account/ChangePassword ─────────────────────────
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _users.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        // Verify current password
        var isCurrentPasswordValid = await _users.CheckPasswordAsync(user, model.CurrentPassword);
        if (!isCurrentPasswordValid)
        {
            ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
            return View(model);
        }

        // Change password
        var result = await _users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (result.Succeeded)
        {
            _log.LogInformation("User {User} changed password successfully", user.UserName);
            
            // Re-sign in to update security stamp
            await _signIn.RefreshSignInAsync(user);
            
            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction(nameof(Profile));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    // ── POST /Account/VerifyPassword ─────────────────────────
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Json(new { success = false, message = "Password is required" });
            }

            var user = await _users.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Verify password
            var isPasswordValid = await _users.CheckPasswordAsync(user, request.Password);
            
            if (isPasswordValid)
            {
                _log.LogInformation("Password verified for user {User} to access sensitive area", user.UserName);
                return Json(new { success = true });
            }
            else
            {
                _log.LogWarning("Failed password verification attempt for user {User}", user.UserName);
                return Json(new { success = false, message = "Incorrect password. Please try again." });
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error during password verification");
            return Json(new { success = false, message = "An error occurred. Please try again." });
        }
    }

}

/// <summary>
/// Request model for password verification
/// </summary>
public class VerifyPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}
