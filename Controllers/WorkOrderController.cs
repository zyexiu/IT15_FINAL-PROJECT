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
    public async Task<IActionResult> Index(string? status = null, string? search = null)
    {
        var query = _db.WorkOrders
            .Include(w => w.Item)
            .Include(w => w.CreatedBy)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(w => w.Status == status);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(w =>
                w.WoNumber.Contains(search) ||
                (w.Item != null && w.Item.ItemName.Contains(search)) ||
                (w.ProductionLine != null && w.ProductionLine.Contains(search))
            );
        }

        var workOrders = await query
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        ViewBag.StatusFilter = status;
        ViewBag.SearchTerm = search;
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

        // Check if QC result exists
        var hasQcResult = await _db.QcResults.AnyAsync(q => q.WorkOrderId == id);
        ViewBag.HasQcResult = hasQcResult;

        // Load downtime reports for this work order
        var downtimeReports = await _db.DowntimeReports
            .Include(d => d.ReportedBy)
            .Include(d => d.ResolvedBy)
            .Where(d => d.WorkOrderId == id)
            .OrderByDescending(d => d.ReportedAt)
            .ToListAsync();
        ViewBag.DowntimeReports = downtimeReports;

        // Calculate total downtime
        var totalDowntime = downtimeReports.Sum(d => d.DurationMinutes);
        ViewBag.TotalDowntime = totalDowntime;

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
        // ═══════════════════════════════════════════════════════
        // VALIDATION: Must have BOM
        // ═══════════════════════════════════════════════════════
        if (model.BomId == 0)
        {
            ModelState.AddModelError("BomId", "BOM is required. You cannot create a work order without a Bill of Materials.");
            await PopulateDropdowns();
            return View(model);
        }

        // Verify BOM exists and is for the selected item
        var bom = await _db.BillsOfMaterials
            .Include(b => b.BomLines)
            .FirstOrDefaultAsync(b => b.BomId == model.BomId);

        if (bom == null)
        {
            ModelState.AddModelError("BomId", "Selected BOM not found.");
            await PopulateDropdowns();
            return View(model);
        }

        if (bom.ItemId != model.ItemId)
        {
            ModelState.AddModelError("BomId", "Selected BOM does not match the selected item.");
            await PopulateDropdowns();
            return View(model);
        }

        // Check if BOM has materials
        if (bom.BomLines == null || !bom.BomLines.Any())
        {
            ModelState.AddModelError("BomId", "Selected BOM has no materials defined. Please add materials to the BOM first.");
            await PopulateDropdowns();
            return View(model);
        }

        // ═══════════════════════════════════════════════════════
        // STOCK CHECKING: Warn if not enough materials
        // ═══════════════════════════════════════════════════════
        var batchMultiplier = model.PlannedQty / bom.BatchOutputQty;
        var shortages = new List<string>();

        foreach (var bomLine in bom.BomLines)
        {
            var requiredQty = bomLine.QtyPerBatch * batchMultiplier;
            var balance = await _db.InventoryBalances
                .Include(b => b.Item)
                .FirstOrDefaultAsync(b => b.ItemId == bomLine.ItemId);

            if (balance == null || (balance.QtyOnHand - balance.QtyReserved) < requiredQty)
            {
                var available = balance?.QtyOnHand - balance?.QtyReserved ?? 0;
                shortages.Add($"{bomLine.Item?.ItemName}: Need {requiredQty:F2}{bomLine.UnitOfMeasure}, Available {available:F2}{bomLine.UnitOfMeasure}");
            }
        }

        if (shortages.Any())
        {
            TempData["Warning"] = $"⚠️ Material Shortage Detected! {string.Join("; ", shortages)}. Work order will be created but you may not have enough materials.";
        }

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
    public async Task<IActionResult> UpdateStatus(int id, string status, decimal? actualQty = null)
    {
        var wo = await _db.WorkOrders
            .Include(w => w.Bom)
                .ThenInclude(b => b!.BomLines!)
                    .ThenInclude(bl => bl.Item)
            .Include(w => w.Item)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id);

        if (wo == null)
            return NotFound();

        var oldStatus = wo.Status;
        wo.Status = status;
        wo.UpdatedAt = DateTime.UtcNow;

        // Set actual start/end times
        if (status == "InProgress" && wo.ActualStart == null)
            wo.ActualStart = DateTime.UtcNow;
        else if (status == "Completed" && wo.ActualEnd == null)
            wo.ActualEnd = DateTime.UtcNow;

        // Update actual quantity if provided
        if (actualQty.HasValue && actualQty.Value > 0)
            wo.ActualQty = actualQty.Value;

        // ═══════════════════════════════════════════════════════
        // AUTOMATIC INVENTORY UPDATES
        // ═══════════════════════════════════════════════════════

        // When RELEASING work order: Reserve materials
        if (status == "Released" && oldStatus == "Draft")
        {
            if (wo.Bom?.BomLines != null)
            {
                foreach (var bomLine in wo.Bom.BomLines)
                {
                    var balance = await _db.InventoryBalances
                        .FirstOrDefaultAsync(b => b.ItemId == bomLine.ItemId);

                    if (balance != null)
                    {
                        // Calculate required quantity based on work order qty
                        var batchMultiplier = wo.PlannedQty / wo.Bom.BatchOutputQty;
                        var requiredQty = bomLine.QtyPerBatch * batchMultiplier;

                        balance.QtyReserved += requiredQty;
                        balance.LastUpdated = DateTime.UtcNow;
                    }
                }
            }
        }

        // When STARTING production: Issue materials (deduct from inventory)
        if (status == "InProgress" && oldStatus == "Released")
        {
            if (wo.Bom?.BomLines != null)
            {
                foreach (var bomLine in wo.Bom.BomLines)
                {
                    var balance = await _db.InventoryBalances
                        .FirstOrDefaultAsync(b => b.ItemId == bomLine.ItemId);

                    if (balance != null)
                    {
                        // Calculate required quantity
                        var batchMultiplier = wo.PlannedQty / wo.Bom.BatchOutputQty;
                        var requiredQty = bomLine.QtyPerBatch * batchMultiplier;

                        // Deduct from on-hand and reserved
                        balance.QtyOnHand -= requiredQty;
                        balance.QtyReserved -= requiredQty;
                        balance.LastUpdated = DateTime.UtcNow;

                        // Create inventory ledger entry
                        var ledger = new InventoryLedger
                        {
                            ItemId = bomLine.ItemId,
                            MovementType = "WoIssue",
                            Qty = -requiredQty,
                            UnitOfMeasure = bomLine.UnitOfMeasure,
                            BalanceAfter = balance.QtyOnHand,
                            WorkOrderId = wo.WorkOrderId,
                            Reference = wo.WoNumber,
                            Notes = $"Materials issued for {wo.WoNumber}",
                            PostedAt = DateTime.UtcNow
                        };
                        _db.InventoryLedgers.Add(ledger);
                    }
                }
            }
        }

        // When COMPLETING work order: Add finished goods to inventory
        if (status == "Completed" && oldStatus == "InProgress")
        {
            if (wo.ActualQty > 0)
            {
                // Find or create inventory balance for finished good
                var balance = await _db.InventoryBalances
                    .FirstOrDefaultAsync(b => b.ItemId == wo.ItemId);

                if (balance == null)
                {
                    // Create new balance if doesn't exist
                    balance = new InventoryBalance
                    {
                        ItemId = wo.ItemId,
                        QtyOnHand = 0,
                        QtyReserved = 0,
                        LastUpdated = DateTime.UtcNow
                    };
                    _db.InventoryBalances.Add(balance);
                    await _db.SaveChangesAsync(); // Save to get BalanceId
                }

                // Add finished goods to inventory
                balance.QtyOnHand += wo.ActualQty;
                balance.LastUpdated = DateTime.UtcNow;

                // Create inventory ledger entry
                var ledger = new InventoryLedger
                {
                    ItemId = wo.ItemId,
                    MovementType = "ProductionOutput",
                    Qty = wo.ActualQty,
                    UnitOfMeasure = wo.UnitOfMeasure,
                    BalanceAfter = balance.QtyOnHand,
                    WorkOrderId = wo.WorkOrderId,
                    Reference = wo.WoNumber,
                    Notes = $"Production output from {wo.WoNumber}",
                    PostedAt = DateTime.UtcNow
                };
                _db.InventoryLedgers.Add(ledger);
            }
            else
            {
                TempData["Warning"] = "Work order completed but no actual quantity recorded. Inventory not updated.";
            }
        }

        await _db.SaveChangesAsync();

        _log.LogInformation("Work order {WoNumber} status changed from {OldStatus} to {NewStatus}", wo.WoNumber, oldStatus, status);
        TempData["Success"] = $"Work order {wo.WoNumber} status updated to {status}. Inventory automatically updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── POST /WorkOrder/Delete/5 ─────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Delete(int id)
    {
        var wo = await _db.WorkOrders
            .Include(w => w.Materials)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id);

        if (wo == null)
            return NotFound();

        var woNumber = wo.WoNumber;

        // Safety check: Only allow deletion of Draft or Cancelled work orders
        if (wo.Status != "Draft" && wo.Status != "Cancelled")
        {
            TempData["Error"] = $"Cannot delete work order {woNumber}. Only Draft or Cancelled work orders can be deleted. Current status: {wo.Status}";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            // Delete related materials first
            if (wo.Materials != null && wo.Materials.Any())
            {
                _db.WorkOrderMaterials.RemoveRange(wo.Materials);
            }

            // Delete work order
            _db.WorkOrders.Remove(wo);
            await _db.SaveChangesAsync();

            _log.LogInformation("Work order {WoNumber} deleted", woNumber);
            TempData["Success"] = $"Work order {woNumber} has been permanently deleted.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error deleting work order {WoNumber}", woNumber);
            TempData["Error"] = $"An error occurred while deleting work order {woNumber}. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // ── GET /WorkOrder/LogProduction/5 ──────────────────────
    [Authorize(Roles = "Operator,Admin")]
    public async Task<IActionResult> LogProduction(int id)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var wo = await _db.WorkOrders
            .Include(w => w.Item)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id);

        if (wo == null) return NotFound();

        // Only allow logging for Released or InProgress work orders
        if (wo.Status != "Released" && wo.Status != "InProgress")
        {
            TempData["Error"] = "Production can only be logged for Released or In Progress work orders.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Get existing production logs
        var logs = await _db.ProductionLogs
            .Include(l => l.RecordedBy)
            .Where(l => l.WorkOrderId == id)
            .OrderByDescending(l => l.LogDate)
            .ThenByDescending(l => l.CreatedAt)
            .ToListAsync();

        ViewBag.WorkOrder = wo;
        ViewBag.ProductionLogs = logs;
        ViewBag.TotalProduced = logs.Sum(l => l.ProducedQty);
        ViewBag.TotalScrap = logs.Sum(l => l.ScrapQty);
        ViewBag.RemainingQty = wo.PlannedQty - logs.Sum(l => l.ProducedQty);

        return View();
    }

    // ── POST /WorkOrder/LogProduction ───────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Operator,Admin")]
    public async Task<IActionResult> LogProduction(int workOrderId, decimal producedQty, decimal scrapQty, string shift, decimal laborHours, decimal machineHours, string? notes)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var wo = await _db.WorkOrders
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId);

        if (wo == null) return NotFound();

        // Validate quantities
        if (producedQty <= 0)
        {
            TempData["Error"] = "Produced quantity must be greater than zero.";
            return RedirectToAction(nameof(LogProduction), new { id = workOrderId });
        }

        // Create production log
        var log = new ProductionLog
        {
            WorkOrderId = workOrderId,
            LogDate = DateTime.UtcNow,
            Shift = shift,
            ProducedQty = producedQty,
            ScrapQty = scrapQty,
            UnitOfMeasure = wo.UnitOfMeasure,
            LaborHours = laborHours,
            MachineHours = machineHours,
            Notes = notes,
            RecordedByUserId = user.Id,
            TenantId = user.TenantId ?? user.Id
        };

        _db.ProductionLogs.Add(log);

        // Update work order actual quantity
        wo.ActualQty += producedQty;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Production logged: {producedQty:N2} {wo.UnitOfMeasure} produced.";
        return RedirectToAction(nameof(LogProduction), new { id = workOrderId });
    }

    // ── POST /WorkOrder/DeleteLog/5 ─────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteLog(int id, int workOrderId)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var log = await _db.ProductionLogs
            .Include(l => l.WorkOrder)
            .FirstOrDefaultAsync(l => l.LogId == id);

        if (log == null) return NotFound();

        // Update work order actual quantity
        if (log.WorkOrder != null)
        {
            log.WorkOrder.ActualQty -= log.ProducedQty;
        }

        _db.ProductionLogs.Remove(log);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Production log deleted successfully.";
        return RedirectToAction(nameof(LogProduction), new { id = workOrderId });
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

        // BOMs with ItemId for filtering
        var boms = await _db.BillsOfMaterials
            .Include(b => b.Item)
            .Where(b => b.IsActive)
            .Select(b => new
            {
                b.BomId,
                b.ItemId,
                BomName = b.Item!.ItemName + " - " + b.Version
            })
            .ToListAsync();

        ViewBag.Boms = new SelectList(boms, "BomId", "BomName");
        
        // Pass BOM data as JSON for JavaScript filtering
        ViewBag.BomData = System.Text.Json.JsonSerializer.Serialize(boms);

        ViewBag.Statuses = new SelectList(new[]
        {
            "Draft", "Released", "InProgress", "Completed", "Cancelled"
        });
    }
}
