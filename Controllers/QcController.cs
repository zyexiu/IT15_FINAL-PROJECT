using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.Models;
using SnackFlowMES.Services;

namespace SnackFlowMES.Controllers;

[Authorize(Roles = "Admin,QC")]
public class QcController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly NotificationService _notifications;
    private readonly ILogger<QcController> _log;

    public QcController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        NotificationService notifications,
        ILogger<QcController> log)
    {
        _db = db;
        _users = users;
        _notifications = notifications;
        _log = log;
    }

    // ── GET /Qc/RecordInspection/5 ───────────────────────────
    public async Task<IActionResult> RecordInspection(int workOrderId)
    {
        var workOrder = await _db.WorkOrders
            .Include(w => w.Item)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId);

        if (workOrder == null)
            return NotFound();

        // Check if already inspected
        var existingQc = await _db.QcResults
            .FirstOrDefaultAsync(q => q.WorkOrderId == workOrderId);

        if (existingQc != null)
        {
            TempData["Warning"] = "This work order has already been inspected. You can view the results below.";
            return RedirectToAction(nameof(Details), new { id = existingQc.QcResultId });
        }

        ViewBag.WorkOrder = workOrder;
        PopulateDropdowns();
        return View(new QcResult { WorkOrderId = workOrderId });
    }

    // ── POST /Qc/RecordInspection ────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordInspection(QcResult model)
    {
        // Remove server-set fields from validation — they're assigned below
        ModelState.Remove(nameof(model.InspectedByUserId));
        ModelState.Remove(nameof(model.TenantId));
        ModelState.Remove(nameof(model.InspectedAt));
        ModelState.Remove(nameof(model.CreatedAt));
        ModelState.Remove(nameof(model.WorkOrder));
        ModelState.Remove(nameof(model.InspectedBy));

        if (!ModelState.IsValid)
        {
            var wo = await _db.WorkOrders.Include(w => w.Item).FirstOrDefaultAsync(w => w.WorkOrderId == model.WorkOrderId);
            ViewBag.WorkOrder = wo;
            PopulateDropdowns();
            return View(model);
        }

        var user = await _users.GetUserAsync(User);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            return View(model);
        }

        var workOrder = await _db.WorkOrders
            .Include(w => w.Item)
            .FirstOrDefaultAsync(w => w.WorkOrderId == model.WorkOrderId);

        if (workOrder == null)
        {
            ModelState.AddModelError("", "Work order not found.");
            return View(model);
        }

        model.InspectedByUserId = user.Id;
        model.InspectedAt = DateTime.UtcNow;
        model.CreatedAt = DateTime.UtcNow;
        model.TenantId = user.TenantId ?? user.Id;

        _db.QcResults.Add(model);
        await _db.SaveChangesAsync();

        // Create notifications based on QC result
        if (model.Result == "Fail" || model.Result == "ConditionalPass")
        {
            await _notifications.CreateQCFailedNotificationAsync(model, workOrder, model.TenantId);
        }
        else if (model.Result == "Pass")
        {
            await _notifications.CreateQCPassedNotificationAsync(model, workOrder, model.TenantId);
        }

        _log.LogInformation("QC inspection recorded for WO {WoNumber} by {User}: {Result}", workOrder.WoNumber, user.UserName, model.Result);
        TempData["Success"] = $"QC inspection recorded successfully. Result: {model.Result}";
        return RedirectToAction("Details", "WorkOrder", new { id = model.WorkOrderId });
    }

    // ── GET /Qc/Details/5 ────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var qcResult = await _db.QcResults
            .Include(q => q.WorkOrder)
                .ThenInclude(w => w!.Item)
            .Include(q => q.InspectedBy)
            .FirstOrDefaultAsync(q => q.QcResultId == id);

        if (qcResult == null)
            return NotFound();

        return View(qcResult);
    }

    // ── GET /Qc/Index ────────────────────────────────────────
    public async Task<IActionResult> Index(string? result = null)
    {
        var query = _db.QcResults
            .Include(q => q.WorkOrder)
                .ThenInclude(w => w!.Item)
            .Include(q => q.InspectedBy)
            .AsQueryable();

        if (!string.IsNullOrEmpty(result))
        {
            query = query.Where(q => q.Result == result);
        }

        var qcResults = await query
            .OrderByDescending(q => q.InspectedAt)
            .ToListAsync();

        ViewBag.ResultFilter = result;
        return View(qcResults);
    }

    // ── GET /Qc/Edit/5 ───────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var qcResult = await _db.QcResults
            .Include(q => q.WorkOrder)
                .ThenInclude(w => w!.Item)
            .FirstOrDefaultAsync(q => q.QcResultId == id);

        if (qcResult == null)
            return NotFound();

        ViewBag.WorkOrder = qcResult.WorkOrder;
        PopulateDropdowns();
        return View(qcResult);
    }

    // ── POST /Qc/Edit/5 ──────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, QcResult model)
    {
        if (id != model.QcResultId)
            return NotFound();

        // Remove server-set fields from validation
        ModelState.Remove(nameof(model.InspectedByUserId));
        ModelState.Remove(nameof(model.TenantId));
        ModelState.Remove(nameof(model.InspectedAt));
        ModelState.Remove(nameof(model.CreatedAt));
        ModelState.Remove(nameof(model.WorkOrder));
        ModelState.Remove(nameof(model.InspectedBy));

        if (!ModelState.IsValid)
        {
            var wo = await _db.WorkOrders.Include(w => w.Item).FirstOrDefaultAsync(w => w.WorkOrderId == model.WorkOrderId);
            ViewBag.WorkOrder = wo;
            PopulateDropdowns();
            return View(model);
        }

        var qcResult = await _db.QcResults.FindAsync(id);
        if (qcResult == null)
            return NotFound();

        // Update fields
        qcResult.Result = model.Result;
        qcResult.CheckType = model.CheckType;
        qcResult.SampleQty = model.SampleQty;
        qcResult.DefectQty = model.DefectQty;
        qcResult.Notes = model.Notes;
        qcResult.Disposition = model.Disposition;

        await _db.SaveChangesAsync();

        _log.LogInformation("QC result {QcResultId} updated", id);
        TempData["Success"] = "QC inspection updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Helper: Populate dropdowns ───────────────────────────
    private void PopulateDropdowns()
    {
        ViewBag.Results = new SelectList(new[]
        {
            "Pass",
            "Fail",
            "ConditionalPass"
        });

        ViewBag.CheckTypes = new SelectList(new[]
        {
            "Appearance",
            "Weight",
            "Moisture",
            "Taste",
            "Packaging",
            "Overall"
        });

        ViewBag.Dispositions = new SelectList(new[]
        {
            "Accept",
            "Rework",
            "Reject"
        });
    }
}
