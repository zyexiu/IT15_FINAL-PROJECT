using Microsoft.AspNetCore.Identity;
using SnackFlowMES.Models;
using System.Security.Claims;

namespace SnackFlowMES.Services;

/// <summary>
/// Service to manage tenant context for multi-tenancy.
/// Each Admin is a tenant, and all their users and data belong to that tenant.
/// </summary>
public class TenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    /// <summary>
    /// Gets the current user's TenantId.
    /// For Admins, this is their own ID.
    /// For other roles, this is their Admin's ID.
    /// </summary>
    public async Task<string?> GetCurrentTenantIdAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            return null;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return null;

        var appUser = await _userManager.FindByIdAsync(userId);
        return appUser?.TenantId;
    }

    /// <summary>
    /// Gets the current user's TenantId synchronously (use with caution).
    /// </summary>
    public string? GetCurrentTenantId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            return null;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId;
    }

    /// <summary>
    /// Checks if the current user is an Admin (tenant owner).
    /// </summary>
    public bool IsCurrentUserAdmin()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.IsInRole("Admin") ?? false;
    }
}
