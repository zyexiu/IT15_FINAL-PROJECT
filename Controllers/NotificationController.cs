using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.Models;
using SnackFlowMES.Services;

namespace SnackFlowMES.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly NotificationService _notifications;
    private readonly ILogger<NotificationController> _log;

    public NotificationController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        NotificationService notifications,
        ILogger<NotificationController> log)
    {
        _db = db;
        _users = users;
        _notifications = notifications;
        _log = log;
    }

    // ── POST /Notification/MarkAsRead/{id} ──────────────────
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var notification = await _db.Notifications.FindAsync(id);
        if (notification == null)
            return NotFound();

        // Verify user has permission to mark this notification as read
        if (notification.RecipientUserId != user.Id && 
            (string.IsNullOrEmpty(notification.RecipientRole) || !User.IsInRole(notification.RecipientRole)))
        {
            return Forbid();
        }

        await _notifications.MarkAsReadAsync(id);
        _log.LogInformation("Notification {NotificationId} marked as read by {User}", id, user.UserName);

        // Redirect back to referrer or Material Request page
        var referrer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referrer))
            return Redirect(referrer);

        return RedirectToAction("Index", "MaterialRequest");
    }

    // ── POST /Notification/MarkAllAsRead ────────────────────
    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var roles = await _users.GetRolesAsync(user);
        var role = roles.FirstOrDefault();

        // Resolve tenant ID
        var tenantId = user.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            tenantId = roles.Contains("Admin") ? user.Id : user.Id;
        }

        await _notifications.MarkAllAsReadAsync(user.Id, role, tenantId);
        _log.LogInformation("All notifications marked as read for user {User}", user.UserName);

        var referrer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referrer))
            return Redirect(referrer);

        return Ok();
    }

    // ── GET /Notification/GetUnreadCount ────────────────────
    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var roles = await _users.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        // Resolve tenant ID
        var tenantId = user.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            tenantId = roles.Contains("Admin") ? user.Id : user.Id;
        }

        // Get unread count for user
        var userCount = await _notifications.GetUnreadCountAsync(user.Id, tenantId);

        // Get unread count for role
        var roleCount = await _notifications.GetUnreadRoleCountAsync(role, tenantId);

        var totalCount = userCount + roleCount;

        _log.LogInformation("GetUnreadCount - User: {UserId}, Role: {Role}, TenantId: {TenantId}", user.Id, role, tenantId);
        _log.LogInformation("GetUnreadCount - UserCount: {UserCount}, RoleCount: {RoleCount}, Total: {Total}", userCount, roleCount, totalCount);

        return Json(new { count = totalCount, userCount, roleCount });
    }
}
