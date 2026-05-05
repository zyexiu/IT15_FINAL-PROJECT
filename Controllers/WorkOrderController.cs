using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.Models;
using SnackFlowMES.ViewModels;

namespace SnackFlowMES.Controllers;

[Authorize]
public class WorkOrderController : Controller
{
    private readonly ApplicationDbContext          _db;
    private readonly UserManager<ApplicationUser>  _users;
    private readonly ILogger<WorkOrderController>  _log;

    public WorkOrderController(
        ApplicationDbContext         db,
        UserManager<ApplicationUser> users,
        ILogger<WorkOrderController> log)
    {
        _db    = db;
        _users = users;
        _log   = log;
    }

    // ── GET /WorkOrder ───────────────────────────────────────
    public async Task<IActionResult> Index(string? status = null)
    {
        var query = _db.WorkOrders
            .Include(w => w.Item)
            .Include(w => w.CreatedBy)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(w => w.Status == status);

        var workOrders = await query
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        ViewBag.StatusFilter = status;
        return View(workOrders);
    }

    // ── GET /WorkOrder/Details/5 ─────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var wo = await _db.WorkOrders
            .Include(w => w.Item)
            .Include(w => w.Bom)
            .Include(w => w.CreatedBy)
            .Include(w => w.Materials!)
                .ThenInclude(m => m.Item)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id);

        if (wo == null)
            return NotFound();

        return View(wo);
    }

    // ── GET /WorkOrder/Create ────────────────────────────────
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new WorkOrderFormViewModel());
    }

    // ── POST /WorkOrder/Create ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Create(WorkOrderFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var user = await _users.GetUserAsync(User);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            await PopulateDropdowns();
            return View(model);
        }

        // Generate WO number
        var lastWo = await _db.WorkOrders
            .OrderByDescending(w => w.WorkOrderId)
            .FirstOrDefaultAsync();
        var nextNum = (lastWo?.WorkOrderId ?? 0) + 1;
        var woNumber = $"WO-{DateTime.Now.Year}-{nextNum:D4}";

        var workOrder = new WorkOrder
        {
            WoNumber        = woNumber,
            ItemId          = model.ItemId,
            BomId           = model.BomId,
            PlannedQty      = model.PlannedQty,
            UnitOfMeasure   = model.UnitOfMeasure,
            Status          = model.Status,
            ProductionLine  = model.ProductionLine,
            ScheduledStart  = model.ScheduledStart,
            ScheduledEnd    = model.ScheduledEnd,
            Notes           = model.Notes,
            CreatedByUserId = user.Id,
            CreatedAt       = DateTime.UtcNow,
            UpdatedAt       = DateTime.UtcNow
        };

        _db.WorkOrders.Add(workOrder);
        await _db.SaveChangesAsync();

        _log.LogInformation("Work order {WoNumber} created by {User}", woNumber, user.UserName);
        TempData["Success"] = $"Work order {woNumber} created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /WorkOrder/Edit/5 ────────────────────────────────
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Edit(int id)
    {
        var wo = await _db.WorkOrders.FindAsync(id);
        if (wo == null)
            return NotFound();

        var model = new WorkOrderFormViewModel
        {
            WorkOrderId    = wo.WorkOrderId,
            ItemId         = wo.ItemId,
            BomId          = wo.BomId,
            PlannedQty     = wo.PlannedQty,
            UnitOfMeasure  = wo.UnitOfMeasure,
            Status         = wo.Status,
            ProductionLine = wo.ProductionLine,
            ScheduledStart = wo.ScheduledStart,
            ScheduledEnd   = wo.ScheduledEnd,
            Notes          = wo.Notes
        };

        await PopulateDropdowns();
        return View(model);
    }

    // ── POST /WorkOrder/Edit/5 ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Edit(int id, WorkOrderFormViewModel model)
    {
        if (id != model.WorkOrderId)
            return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(model);
        }

        var wo = await _db.WorkOrders.FindAsync(id);
        if (wo == null)
            return NotFound();

        wo.ItemId         = model.ItemId;
        wo.BomId          = model.BomId;
        wo.PlannedQty     = model.PlannedQty;
        wo.UnitOfMeasure  = model.UnitOfMeasure;
        wo.Status         = model.Status;
        wo.ProductionLine = model.ProductionLine;
        wo.ScheduledStart = model.ScheduledStart;
        wo.ScheduledEnd   = model.ScheduledEnd;
        wo.Notes          = model.Notes;
        wo.UpdatedAt      = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _log.LogInformation("Work order {WoNumber} updated", wo.WoNumber);
        TempData["Success"] = $"Work order {wo.WoNumber} updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /WorkOrder/UpdateStatus ─────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var wo = await _db.WorkOrders.FindAsync(id);
        if (wo == null)
            return NotFound();

        wo.Status = status;
        wo.UpdatedAt = DateTime.UtcNow;

        // Set actual start/end times
        if (status == "InProgress" && wo.ActualStart == null)
            wo.ActualStart = DateTime.UtcNow;
        else if (status == "Completed" && wo.ActualEnd == null)
            wo.ActualEnd = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _log.LogInformation("Work order {WoNumber} status changed to {Status}", wo.WoNumber, status);
        TempData["Success"] = $"Work order {wo.WoNumber} status updated to {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Helper: Populate dropdowns ───────────────────────────
    private async Task PopulateDropdowns()
    {
        // Finished goods only
        ViewBag.Items = new SelectList(
            await _db.Items
                .Where(i => i.ItemType == "FinishedGood" && i.IsActive)
                .OrderBy(i => i.ItemName)
                .ToListAsync(),
            "ItemId", "ItemName");

        // BOMs
        ViewBag.Boms = new SelectList(
            await _db.BillsOfMaterials
                .Include(b => b.Item)
                .Where(b => b.IsActive)
                .Select(b => new { b.BomId, BomName = b.Item!.ItemName + " - " + b.Version })
                .ToListAsync(),
            "BomId", "BomName");

        ViewBag.Statuses = new SelectList(new[]
        {
            "Draft", "Released", "InProgress", "Completed", "Cancelled"
        });
    }
}
