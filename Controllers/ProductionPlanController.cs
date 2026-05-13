using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.Models;
using SnackFlowMES.ViewModels;

namespace SnackFlowMES.Controllers;

[Authorize]
public class ProductionPlanController : Controller
{
    private readonly ApplicationDbContext          _db;
    private readonly UserManager<ApplicationUser>  _users;

    public ProductionPlanController(
        ApplicationDbContext         db,
        UserManager<ApplicationUser> users)
    {
        _db    = db;
        _users = users;
    }

    // ── GET /ProductionPlan ──────────────────────────────────
    public async Task<IActionResult> Index(string? status, string? search)
    {
        // Start with base query
        var query = _db.ProductionPlans
            .Include(p => p.CreatedBy)
            .Include(p => p.PlanLines!)
                .ThenInclude(l => l.Item)
            .AsQueryable();

        // Apply status filter if provided
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            query = query.Where(p => p.Status == status);
        }

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.PlanName.Contains(search) ||
                (p.Notes != null && p.Notes.Contains(search))
            );
        }

        // Order by created date (newest first)
        var plans = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        // Pass current filter to view
        ViewBag.CurrentStatus = status ?? "All";
        ViewBag.SearchTerm = search;
        ViewBag.StatusOptions = new List<string> 
        { 
            "All",
            "Draft",
            "Approved", 
            "InProgress", 
            "Completed", 
            "Cancelled" 
        };

        return View(plans);
    }

    // ── GET /ProductionPlan/Create ───────────────────────────
    [Authorize(Roles = "Admin,Planner")]
    public IActionResult Create()
    {
        return View(new ProductionPlanFormViewModel());
    }

    // ── POST /ProductionPlan/Create ──────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Create(ProductionPlanFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Validate date range
        if (model.PlanDateTo <= model.PlanDateFrom)
        {
            ModelState.AddModelError("PlanDateTo", "End date must be after start date");
            return View(model);
        }

        var user = await _users.GetUserAsync(User);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            return View(model);
        }

        var productionPlan = new ProductionPlan
        {
            PlanName = model.PlanName,
            PlanDateFrom = model.PlanDateFrom,
            PlanDateTo = model.PlanDateTo,
            Status = model.Status,
            Notes = model.Notes,
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ProductionPlans.Add(productionPlan);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Production plan '{model.PlanName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /ProductionPlan/Details/5 ────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var plan = await _db.ProductionPlans
            .Include(p => p.CreatedBy)
            .Include(p => p.PlanLines!)
                .ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(p => p.PlanId == id);

        if (plan == null)
            return NotFound();

        return View(plan);
    }

    // ── GET /ProductionPlan/Edit/5 ───────────────────────────
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Edit(int id)
    {
        var plan = await _db.ProductionPlans.FindAsync(id);
        if (plan == null)
            return NotFound();

        var model = new ProductionPlanFormViewModel
        {
            PlanId = plan.PlanId,
            PlanName = plan.PlanName,
            PlanDateFrom = plan.PlanDateFrom,
            PlanDateTo = plan.PlanDateTo,
            Status = plan.Status,
            Notes = plan.Notes
        };

        return View(model);
    }

    // ── POST /ProductionPlan/Edit/5 ──────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Edit(int id, ProductionPlanFormViewModel model)
    {
        if (id != model.PlanId)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        // Validate date range
        if (model.PlanDateTo <= model.PlanDateFrom)
        {
            ModelState.AddModelError("PlanDateTo", "End date must be after start date");
            return View(model);
        }

        var plan = await _db.ProductionPlans.FindAsync(id);
        if (plan == null)
            return NotFound();

        plan.PlanName = model.PlanName;
        plan.PlanDateFrom = model.PlanDateFrom;
        plan.PlanDateTo = model.PlanDateTo;
        plan.Status = model.Status;
        plan.Notes = model.Notes;
        plan.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Production plan '{model.PlanName}' updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── POST /ProductionPlan/Delete/5 ────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> Delete(int id)
    {
        var plan = await _db.ProductionPlans
            .Include(p => p.PlanLines)
            .FirstOrDefaultAsync(p => p.PlanId == id);

        if (plan == null)
            return NotFound();

        var planName = plan.PlanName;

        // Delete plan lines first
        if (plan.PlanLines != null && plan.PlanLines.Any())
        {
            _db.PlanLines.RemoveRange(plan.PlanLines);
        }

        // Delete plan
        _db.ProductionPlans.Remove(plan);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Production plan '{planName}' deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /ProductionPlan/AddLine/5 ────────────────────────
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> AddLine(int id)
    {
        var plan = await _db.ProductionPlans.FindAsync(id);
        if (plan == null)
            return NotFound();

        ViewBag.PlanName = plan.PlanName;
        ViewBag.PlanId = id;
        
        await PopulateItemsDropdown();

        return View(new PlanLineFormViewModel { PlanId = id });
    }

    // ── POST /ProductionPlan/AddLine ─────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> AddLine(PlanLineFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var plan = await _db.ProductionPlans.FindAsync(model.PlanId);
            ViewBag.PlanName = plan?.PlanName;
            ViewBag.PlanId = model.PlanId;
            await PopulateItemsDropdown();
            return View(model);
        }

        var planLine = new PlanLine
        {
            PlanId = model.PlanId,
            ItemId = model.ItemId,
            PlannedQty = model.PlannedQty,
            UnitOfMeasure = model.UnitOfMeasure,
            ScheduledDate = model.ScheduledDate,
            ProductionLine = model.ProductionLine,
            Status = "Pending",
            Notes = model.Notes
        };

        _db.PlanLines.Add(planLine);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Plan line added successfully.";
        return RedirectToAction(nameof(Details), new { id = model.PlanId });
    }

    // ── GET /ProductionPlan/EditLine/5 ───────────────────────
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> EditLine(int id)
    {
        var line = await _db.PlanLines
            .Include(l => l.Item)
            .FirstOrDefaultAsync(l => l.PlanLineId == id);

        if (line == null) return NotFound();

        var plan = await _db.ProductionPlans.FindAsync(line.PlanId);
        ViewBag.PlanName = plan?.PlanName;

        await PopulateItemsDropdown();

        var model = new PlanLineFormViewModel
        {
            PlanLineId     = line.PlanLineId,
            PlanId         = line.PlanId,
            ItemId         = line.ItemId,
            PlannedQty     = line.PlannedQty,
            UnitOfMeasure  = line.UnitOfMeasure,
            ScheduledDate  = line.ScheduledDate,
            ProductionLine = line.ProductionLine,
            Notes          = line.Notes
        };

        return View(model);
    }

    // ── POST /ProductionPlan/EditLine ─────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> EditLine(PlanLineFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var plan = await _db.ProductionPlans.FindAsync(model.PlanId);
            ViewBag.PlanName = plan?.PlanName;
            await PopulateItemsDropdown();
            return View(model);
        }

        var line = await _db.PlanLines.FindAsync(model.PlanLineId);
        if (line == null) return NotFound();

        line.ItemId        = model.ItemId;
        line.PlannedQty    = model.PlannedQty;
        line.UnitOfMeasure = model.UnitOfMeasure;
        line.ScheduledDate = model.ScheduledDate;
        line.ProductionLine = model.ProductionLine;
        line.Notes         = model.Notes;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Plan line updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.PlanId });
    }

    // ── POST /ProductionPlan/DeleteLine ──────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> DeleteLine(int id, int planId)
    {
        var planLine = await _db.PlanLines.FindAsync(id);
        if (planLine == null)
            return NotFound();

        _db.PlanLines.Remove(planLine);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Plan line deleted successfully.";
        return RedirectToAction(nameof(Details), new { id = planId });
    }

    // ── POST /ProductionPlan/GenerateWorkOrders/5 ────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
    public async Task<IActionResult> GenerateWorkOrders(int id)
    {
        var plan = await _db.ProductionPlans
            .Include(p => p.PlanLines!)
                .ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(p => p.PlanId == id);

        if (plan == null)
            return NotFound();

        if (plan.PlanLines == null || !plan.PlanLines.Any())
        {
            TempData["Error"] = "Cannot generate work orders. The plan has no plan lines.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var user = await _users.GetUserAsync(User);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(Details), new { id });
        }

        int workOrdersCreated = 0;
        var errors = new List<string>();

        foreach (var line in plan.PlanLines.Where(l => l.Status == "Pending"))
        {
            // Find active BOM for this item
            var bom = await _db.BillsOfMaterials
                .Where(b => b.ItemId == line.ItemId && b.IsActive)
                .OrderByDescending(b => b.Version)
                .FirstOrDefaultAsync();

            if (bom == null)
            {
                errors.Add($"No active BOM found for {line.Item?.ItemName}. Skipped.");
                continue;
            }

            // Check if BOM has materials
            var bomHasMaterials = await _db.BomLines
                .AnyAsync(bl => bl.BomId == bom.BomId);

            if (!bomHasMaterials)
            {
                errors.Add($"BOM for {line.Item?.ItemName} has no materials. Skipped.");
                continue;
            }

            // Generate WO number
            var lastWo = await _db.WorkOrders
                .OrderByDescending(w => w.WorkOrderId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastWo != null && lastWo.WoNumber.StartsWith("WO-"))
            {
                var parts = lastWo.WoNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            var woNumber = $"WO-{DateTime.UtcNow.Year}-{nextNumber:D4}";

            // Create work order
            var workOrder = new WorkOrder
            {
                WoNumber = woNumber,
                ItemId = line.ItemId,
                BomId = bom.BomId,
                PlannedQty = line.PlannedQty,
                UnitOfMeasure = line.UnitOfMeasure,
                Status = "Draft",
                ProductionLine = line.ProductionLine,
                ScheduledStart = line.ScheduledDate,
                ScheduledEnd = line.ScheduledDate.AddHours(8), // Default 8-hour shift
                Notes = $"Generated from plan: {plan.PlanName}",
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TenantId = user.TenantId ?? user.Id
            };

            _db.WorkOrders.Add(workOrder);
            workOrdersCreated++;

            // Update plan line status
            line.Status = "WorkOrderCreated";
        }

        await _db.SaveChangesAsync();

        if (workOrdersCreated > 0)
        {
            TempData["Success"] = $"{workOrdersCreated} work order(s) generated successfully.";
        }

        if (errors.Any())
        {
            TempData["Warning"] = string.Join(" ", errors);
        }

        if (workOrdersCreated == 0 && !errors.Any())
        {
            TempData["Info"] = "No pending plan lines to generate work orders from.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Helper: Populate items dropdown ──────────────────────
    private async Task PopulateItemsDropdown()
    {
        var items = await _db.Items
            .Where(i => i.ItemType == "FinishedGood" && i.IsActive)
            .OrderBy(i => i.ItemName)
            .Select(i => new { i.ItemId, i.ItemName })
            .ToListAsync();

        ViewBag.Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(items, "ItemId", "ItemName");
    }
}
