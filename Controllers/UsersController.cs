using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Models;
using SnackFlowMES.ViewModels;

namespace SnackFlowMES.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser>  _users;
    private readonly RoleManager<IdentityRole>     _roles;
    private readonly ILogger<UsersController>      _log;

    public UsersController(
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole>    roles,
        ILogger<UsersController>     log)
    {
        _users = users;
        _roles = roles;
        _log   = log;
    }

    // ── GET /Users ───────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var users = await _users.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var userList = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _users.GetRolesAsync(user);
            userList.Add(new UserListViewModel
            {
                UserId    = user.Id,
                UserName  = user.UserName ?? "",
                Email     = user.Email ?? "",
                FullName  = user.FullName,
                Role      = roles.FirstOrDefault() ?? "—",
                IsActive  = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        return View(userList);
    }

    // ── GET /Users/Create ────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        await PopulateRoles();
        return View(new UserFormViewModel());
    }

    // ── POST /Users/Create ───────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateRoles();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName  = model.Email,
            Email     = model.Email,
            FullName  = model.FullName,
            IsActive  = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _users.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            await PopulateRoles();
            return View(model);
        }

        // Assign role
        if (!string.IsNullOrEmpty(model.Role))
        {
            await _users.AddToRoleAsync(user, model.Role);
        }

        _log.LogInformation("User {Email} created by admin", user.Email);
        TempData["Success"] = $"User {user.Email} created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Users/Edit/id ───────────────────────────────────
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _users.GetRolesAsync(user);

        var model = new UserFormViewModel
        {
            UserId   = user.Id,
            Email    = user.Email ?? "",
            FullName = user.FullName,
            Role     = roles.FirstOrDefault() ?? ""
        };

        await PopulateRoles();
        return View(model);
    }

    // ── POST /Users/Edit/id ──────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserFormViewModel model)
    {
        if (id != model.UserId)
            return NotFound();

        // Remove password validation for edit
        ModelState.Remove("Password");
        ModelState.Remove("ConfirmPassword");

        if (!ModelState.IsValid)
        {
            await PopulateRoles();
            return View(model);
        }

        var user = await _users.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.Email    = model.Email;
        user.UserName = model.Email;
        user.FullName = model.FullName;

        var result = await _users.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            await PopulateRoles();
            return View(model);
        }

        // Update role
        var currentRoles = await _users.GetRolesAsync(user);
        if (currentRoles.Any())
            await _users.RemoveFromRolesAsync(user, currentRoles);

        if (!string.IsNullOrEmpty(model.Role))
            await _users.AddToRoleAsync(user, model.Role);

        _log.LogInformation("User {Email} updated by admin", user.Email);
        TempData["Success"] = $"User {user.Email} updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Users/ToggleStatus ─────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        // Prevent admin from deactivating themselves
        var currentUser = await _users.GetUserAsync(User);
        if (currentUser?.Id == user.Id)
        {
            TempData["Error"] = "You cannot deactivate your own account.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;
        await _users.UpdateAsync(user);

        var status = user.IsActive ? "activated" : "deactivated";
        _log.LogInformation("User {Email} {Status} by admin", user.Email, status);
        TempData["Success"] = $"User {user.Email} has been {status}.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Users/ResetPassword ────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            TempData["Error"] = "Password cannot be empty.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _users.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var result = await _users.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        _log.LogInformation("Password reset for user {Email} by admin", user.Email);
        TempData["Success"] = $"Password reset successfully for {user.Email}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRoles()
    {
        var roles = await _roles.Roles
            .Where(r => r.Name != "Admin") // Exclude Admin role from dropdown
            .Select(r => r.Name!)
            .ToListAsync();

        roles.Insert(0, "Admin"); // Add Admin at the beginning

        ViewBag.Roles = new SelectList(roles);
    }
}
