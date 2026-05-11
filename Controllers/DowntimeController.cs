using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.Models;
using SnackFlowMES.Services;

namespace SnackFlowMES.Controllers;

[Authorize]
public class DowntimeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly NotificationService _notifications;

    public DowntimeController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        NotificationService notifications)
    {
        _db = db;
        _users = users;
        _notifications = notifications;
    }

    // ── GET /Downtime ────────────────────────────────────────
    [Authorize(Roles = "Admin,Manager,Operator")]
    public async Task<IActionResult> Index(string? status = null)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organizationId = user.TenantId ?? user.Id;

        var query = _db.DowntimeReports
            .Include(d => d.WorkOrder)
                .ThenInclude(w => w!.Item)
            .Include(d => d.ReportedBy)
            .Include(d => d.ResolvedBy)
            .Where(d => d.OrganizationId == organizationId)
            .AsQueryable();

        // Filter by status if provided
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(d => d.Status == status);
        }

        var reports = await query
            .OrderByDescending(d => d.ReportedAt)
            .ToListAsync();

        ViewBag.StatusFilter = status;
        return View(reports);
    }

    // ── GET /Downtime/Details/5 ──────────────────────────────
    [Authorize(Roles = "Admin,Manager,Operator")]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organizationId = user.TenantId ?? user.Id;

        var report = await _db.DowntimeReports
            .Include(d => d.WorkOrder)
                .ThenInclude(w => w!.Item)
            .Include(d => d.ReportedBy)
            .Include(d => d.ResolvedBy)
            .FirstOrDefaultAsync(d => d.DowntimeId == id && d.OrganizationId == organizationId);

        if (report == null) return NotFound();

        return View(report);
    }

    // ── GET /Downtime/Create?workOrderId=5 ───────────────────
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> Create(int? workOrderId)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organizationId = user.TenantId ?? user.Id;

        // Load work orders for dropdown
        ViewBag.WorkOrders = await _db.WorkOrders
            .Where(w => w.TenantId == organizationId && 
                       (w.Status == "Released" || w.Status == "InProgress"))
            .Include(w => w.Item)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        // Pre-fill work order if provided
        if (workOrderId.HasValue)
        {
            var wo = await _db.WorkOrders
                .Include(w => w.Item)
                .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId.Value && w.TenantId == organizationId);
            
            if (wo != null)
            {
                ViewBag.SelectedWorkOrder = wo;
            }
        }

        return View();
    }

    // ── POST /Downtime/Create ────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> Create(DowntimeReport model)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organizationId = user.TenantId ?? user.Id;

        // Remove validation for navigation properties
        ModelState.Remove(nameof(model.WorkOrder));
        ModelState.Remove(nameof(model.ReportedBy));
        ModelState.Remove(nameof(model.ResolvedBy));

        // Always derive production line from selected work order (prevents manual tampering)
        var workOrder = await _db.WorkOrders
            .FirstOrDefaultAsync(w => w.WorkOrderId == model.WorkOrderId && w.TenantId == organizationId);

        if (workOrder == null)
        {
            ModelState.AddModelError(nameof(model.WorkOrderId), "Selected work order is invalid.");
        }
        else
        {
            model.ProductionLine = workOrder.ProductionLine ?? string.Empty;
            if (string.IsNullOrWhiteSpace(model.ProductionLine))
            {
                ModelState.AddModelError(nameof(model.ProductionLine), "Selected work order does not have a production line.");
            }
        }

        if (!ModelState.IsValid)
        {
            // Log validation errors for debugging
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine($"[Downtime Create Validation Error] {error.ErrorMessage}");
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                model.ReportedByUserId = user.Id;
                model.ReportedAt = DateTime.UtcNow;
                model.Status = "Open";
                model.OrganizationId = organizationId;

                // Calculate duration if end time is provided
                if (model.EndTime.HasValue)
                {
                    model.DurationMinutes = (int)(model.EndTime.Value - model.StartTime).TotalMinutes;
                }

                _db.DowntimeReports.Add(model);
                await _db.SaveChangesAsync();

                // Create notification for Admin and Manager
                await _notifications.CreateDowntimeReportNotificationAsync(
                    model.DowntimeId,
                    model.WorkOrderId,
                    model.Reason,
                    model.DurationMinutes,
                    user.TenantId ?? user.Id
                );

                TempData["Success"] = "Downtime report submitted successfully.";
                return RedirectToAction(nameof(Details), new { id = model.DowntimeId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving downtime report: {ex.Message}");
                Console.WriteLine($"[Downtime Create Exception] {ex}");
            }
        }

        // Reload work orders for dropdown
        ViewBag.WorkOrders = await _db.WorkOrders
            .Where(w => w.TenantId == (user.TenantId ?? user.Id) && 
                       (w.Status == "Released" || w.Status == "InProgress"))
            .Include(w => w.Item)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return View(model);
    }

    // ── GET /Downtime/Resolve/5 ──────────────────────────────
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Resolve(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organizationId = user.TenantId ?? user.Id;

        var report = await _db.DowntimeReports
            .Include(d => d.WorkOrder)
                .ThenInclude(w => w!.Item)
            .Include(d => d.ReportedBy)
            .FirstOrDefaultAsync(d => d.DowntimeId == id && d.OrganizationId == organizationId);

        if (report == null) return NotFound();

        return View(report);
    }

    // ── POST /Downtime/Resolve/5 ─────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Resolve(int id, string resolution, string status, DateTime? endTime)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organizationId = user.TenantId ?? user.Id;

        var report = await _db.DowntimeReports
            .FirstOrDefaultAsync(d => d.DowntimeId == id && d.OrganizationId == organizationId);

        if (report == null) return NotFound();

        report.Resolution = resolution;
        report.Status = status;
        report.ResolvedByUserId = user.Id;
        report.ResolvedAt = DateTime.UtcNow;

        // Update end time if provided
        if (endTime.HasValue && !report.EndTime.HasValue)
        {
            report.EndTime = endTime.Value;
            report.DurationMinutes = (int)(report.EndTime.Value - report.StartTime).TotalMinutes;
        }

        await _db.SaveChangesAsync();

        // Notify the operator who reported it
        await _notifications.CreateNotificationAsync(
            recipientUserId: report.ReportedByUserId,
            type: "DowntimeResolved",
            title: "Downtime Report Resolved",
            message: $"Your downtime report for {report.ProductionLine} has been {status.ToLower()}.",
            priority: "Medium",
            actionUrl: $"/Downtime/Details/{report.DowntimeId}",
            organizationId: organizationId
        );

        TempData["Success"] = "Downtime report updated successfully.";
        return RedirectToAction(nameof(Details), new { id = report.DowntimeId });
    }

    // ── POST /Downtime/Delete/5 ──────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var organizationId = user.TenantId ?? user.Id;

        var report = await _db.DowntimeReports
            .FirstOrDefaultAsync(d => d.DowntimeId == id && d.OrganizationId == organizationId);

        if (report == null) return NotFound();

        _db.DowntimeReports.Remove(report);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Downtime report deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
