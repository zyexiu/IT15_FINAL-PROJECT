using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.Models;
using SnackFlowMES.Services;

namespace SnackFlowMES.Controllers;

[Authorize]
public class MaterialRequestController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly NotificationService _notifications;
    private readonly ILogger<MaterialRequestController> _log;

    public MaterialRequestController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        NotificationService notifications,
        ILogger<MaterialRequestController> log)
    {
        _db = db;
        _users = users;
        _notifications = notifications;
        _log = log;
    }

    // ── GET /MaterialRequest ─────────────────────────────────
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Index(string? status = null)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var userRoles = await _users.GetRolesAsync(user);
        var userRole = userRoles.FirstOrDefault() ?? "";
        
        _log.LogInformation("📋 Material Request Index - User: {User}, Role: {Role}", user.UserName, userRole);

        // Query requests
        var query = _db.MaterialRequests
            .Include(m => m.Item)
            .Include(m => m.RequestedBy)
            .Include(m => m.ApprovedBy)
            .AsQueryable();

        // Admin sees all requests, Planner sees only their own
        if (userRole == "Planner")
        {
            query = query.Where(m => m.RequestedByUserId == user.Id);
        }

        // Filter by status if provided
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(m => m.Status == status);
        }

        var requests = await query
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();

        _log.LogInformation("📋 Showing {Count} Material Requests", requests.Count);

        ViewBag.StatusFilter = status;
        return View(requests);
    }

    // ── GET /MaterialRequest/Details/5 ───────────────────────
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var request = await _db.MaterialRequests
            .Include(m => m.Item)
            .Include(m => m.RequestedBy)
            .Include(m => m.ApprovedBy)
            .FirstOrDefaultAsync(m => m.RequestId == id);

        if (request == null)
            return NotFound();

        return View(request);
    }

    // ── GET /MaterialRequest/Create ──────────────────────────
    [Authorize(Roles = "Planner")]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns("");
        return View();
    }

    // ── POST /MaterialRequest/Create ─────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Planner")]
    public async Task<IActionResult> Create(MaterialRequest model)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            await PopulateDropdowns("");
            return View(model);
        }

        _log.LogInformation("📝 Material Request Submit - User: {User}, ItemId: {ItemId}, Qty: {Qty}", 
            user.UserName, model.ItemId, model.RequestedQty);

        // Validate required fields
        if (model.ItemId == 0)
        {
            ModelState.AddModelError("ItemId", "Item is required.");
            await PopulateDropdowns("");
            return View(model);
        }

        if (model.RequestedQty <= 0)
        {
            ModelState.AddModelError("RequestedQty", "Quantity must be greater than 0.");
            await PopulateDropdowns("");
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.Reason))
        {
            ModelState.AddModelError("Reason", "Reason is required.");
            await PopulateDropdowns("");
            return View(model);
        }

        if (string.IsNullOrWhiteSpace(model.Priority))
        {
            ModelState.AddModelError("Priority", "Priority is required.");
            await PopulateDropdowns("");
            return View(model);
        }

        // Get item details
        var item = await _db.Items.FindAsync(model.ItemId);
        if (item == null)
        {
            ModelState.AddModelError("ItemId", "Item not found.");
            await PopulateDropdowns("");
            return View(model);
        }

        // Set server fields
        model.UnitOfMeasure = item.UnitOfMeasure;
        model.RequestedByUserId = user.Id;
        model.RequestedAt = DateTime.UtcNow;
        model.Status = "Pending";
        model.TenantId = user.Id;  // Simple: just use requesting user's ID

        try
        {
            _db.MaterialRequests.Add(model);
            await _db.SaveChangesAsync();
            
            _log.LogInformation("✅ Material Request SAVED - RequestId: {RequestId}, Item: {Item}, TenantId: {TenantId}", 
                model.RequestId, item.ItemName, model.TenantId);

            // Create notification
            await _notifications.CreateMaterialRequestNotificationAsync(model, model.TenantId);
            
            _log.LogInformation("✅ Notification CREATED for RequestId: {RequestId}", model.RequestId);

            TempData["Success"] = $"✅ Material request for {item.ItemName} submitted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "❌ ERROR creating material request: {Message}", ex.Message);
            TempData["Error"] = $"Error: {ex.Message}";
            await PopulateDropdowns("");
            return View(model);
        }
    }

    // ── POST /MaterialRequest/Approve/5 ──────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, string? approvalNotes)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var request = await _db.MaterialRequests
            .Include(m => m.Item)
            .Include(m => m.RequestedBy)
            .FirstOrDefaultAsync(m => m.RequestId == id);

        if (request == null)
            return NotFound();

        request.Status = "Approved";
        request.ApprovedByUserId = user.Id;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApprovalNotes = approvalNotes;

        await _db.SaveChangesAsync();

        _log.LogInformation("✅ Material request {RequestId} APPROVED by {User}", id, user.UserName);
        TempData["Success"] = $"✅ Request for {request.Item?.ItemName} approved successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /MaterialRequest/Reject/5 ───────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(int id, string? approvalNotes)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var request = await _db.MaterialRequests
            .Include(m => m.Item)
            .Include(m => m.RequestedBy)
            .FirstOrDefaultAsync(m => m.RequestId == id);

        if (request == null)
            return NotFound();

        request.Status = "Rejected";
        request.ApprovedByUserId = user.Id;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApprovalNotes = approvalNotes;

        await _db.SaveChangesAsync();

        _log.LogInformation("❌ Material request {RequestId} REJECTED by {User}", id, user.UserName);
        TempData["Success"] = $"❌ Request for {request.Item?.ItemName} rejected.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /MaterialRequest/Fulfill/5 ──────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Fulfill(int id, decimal fulfilledQty)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var request = await _db.MaterialRequests
            .Include(m => m.Item)
            .Include(m => m.RequestedBy)
            .FirstOrDefaultAsync(m => m.RequestId == id);

        if (request == null)
            return NotFound();

        if (request.Status != "Approved")
        {
            TempData["Error"] = "Only approved requests can be fulfilled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        request.Status = "Fulfilled";
        request.FulfilledAt = DateTime.UtcNow;
        request.FulfilledQty = fulfilledQty;

        await _db.SaveChangesAsync();

        // Send the fulfilled request back to the planner who created it
        await _notifications.CreateUserNotificationAsync(
            recipientUserId: request.RequestedByUserId,
            type: "MaterialRequest",
            title: "Material Request Fulfilled",
            message: $"Your material request for {request.Item?.ItemName} has been fulfilled by Admin. Qty: {fulfilledQty:N2} {request.UnitOfMeasure}.",
            priority: "Low",
            relatedEntityType: "MaterialRequest",
            relatedEntityId: request.RequestId,
            actionUrl: $"/MaterialRequest/Details/{request.RequestId}",
            tenantId: request.TenantId
        );

        _log.LogInformation("Material request {RequestId} fulfilled and sent back to planner {PlannerId}", id, request.RequestedByUserId);
        TempData["Success"] = "Material request marked as fulfilled.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Helper: Populate dropdowns ───────────────────────────
    private async Task PopulateDropdowns(string tenantId)
    {
        // Simply load ALL active items - no tenant filtering for now
        var items = await _db.Items
            .Where(i => i.IsActive)
            .OrderBy(i => i.ItemName)
            .ToListAsync();

        ViewBag.MaterialItems = items;

        ViewBag.Items = items
            .Select(i => new SelectListItem
            {
                Value = i.ItemId.ToString(),
                Text = $"{i.ItemName} ({i.UnitOfMeasure})"
            })
            .ToList();

        ViewBag.Reasons = new SelectList(new[]
        {
            "LowStock",
            "UpcomingProduction",
            "Emergency",
            "Reorder",
            "Other"
        });

        ViewBag.Priorities = new SelectList(new[]
        {
            "Low",
            "Medium",
            "High",
            "Critical"
        });
    }

    private async Task<string> ResolveTenantIdAsync(ApplicationUser user)
    {
        // Admin: their tenant ID is their own user ID
        var roles = await _users.GetRolesAsync(user);
        if (roles.Contains("Admin"))
            return user.Id;

        // Others: use their TenantId or default to user ID
        return !string.IsNullOrWhiteSpace(user.TenantId) ? user.TenantId : user.Id;
    }
}
