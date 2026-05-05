using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SnackFlowMES.Services;

namespace SnackFlowMES.Filters;

/// <summary>
/// Authorization filter to enforce role-based access control
/// </summary>
public class RoleAccessFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Skip if user is not authenticated
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        // Get controller and action
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
        var action = context.RouteData.Values["action"]?.ToString() ?? "Index";

        // Skip for Account and Home controllers
        if (controller.Equals("Account", StringComparison.OrdinalIgnoreCase) ||
            controller.Equals("Home", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Get user role
        var user = context.HttpContext.User;
        var role = user.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? "";

        // Check access
        if (!RoleMenuService.HasAccess(role, controller, action))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}
