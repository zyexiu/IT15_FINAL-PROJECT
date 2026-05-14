using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.Models;
using SnackFlowMES.ViewModels;

namespace SnackFlowMES.Controllers;

[Authorize(Roles = "Admin,Planner")]
public class BomController : Controller
{
    private readonly ApplicationDbContext          _db;
    private readonly UserManager<ApplicationUser>  _users;
    private readonly ILogger<BomController>        _log;

    public BomController(
        ApplicationDbContext         db,
        UserManager<ApplicationUser> users,
        ILogger<BomController>       log)
    {
        _db    = db;
        _users = users;
        _log   = log;
    }

    // ── GET /BOM ─────────────────────────────────────────────
    public async Task<IActionResult> Index(string? search = null)
    {
        var user  = await _users.GetUserAsync(User);
        var roles = user != null ? await _users.GetRolesAsync(user) : new List<string>();
        var tenantId = user?.TenantId;
        if (user != null && string.IsNullOrWhiteSpace(tenantId))
            tenantId = roles.Contains("Admin") ? user.Id : user.Id;

        var query = _db.BillsOfMaterials
            .Include(b => b.Item)
            .Include(b => b.BomLines!)
                .ThenInclude(l => l.Item)
            .Where(b => b.TenantId == tenantId || string.IsNullOrEmpty(b.TenantId))
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(b => b.Item!.ItemName.Contains(search) || b.Version.Contains(search));

        var boms = await query
            .OrderBy(b => b.Item!.ItemName)
            .ThenBy(b => b.Version)
            .ToListAsync();

        ViewBag.SearchTerm = search;
        return View(boms);
    }

    // ── GET /BOM/Create ─────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        await PopulateFinishedGoods();
        return View(new BomFormViewModel());
    }

    // ── POST /BOM/Create ────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BomFormViewModel model)
    {
        var user  = await _users.GetUserAsync(User);
        var roles = user != null ? await _users.GetRolesAsync(user) : new List<string>();
        var tenantId = user?.TenantId;
        if (user != null && string.IsNullOrWhiteSpace(tenantId))
            tenantId = roles.Contains("Admin") ? user.Id : user.Id;

        // Check if BOM already exists for this item+version
        var existingBom = await _db.BillsOfMaterials
            .FirstOrDefaultAsync(b => b.ItemId == model.ItemId && b.Version == model.Version);
        
        if (existingBom != null)
        {
            ModelState.AddModelError("Version", "A BOM with this version already exists for the selected item.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateFinishedGoods();
            return View(model);
        }

        var bom = new BillOfMaterials
        {
            ItemId         = model.ItemId,
            Version        = model.Version,
            BatchOutputQty = model.BatchOutputQty,
            BatchOutputUom = model.BatchOutputUom,
            EstMachineHours = model.EstMachineHours,
            EstLaborHours  = model.EstLaborHours,
            IsActive       = model.IsActive,
            Notes          = model.Notes,
            TenantId       = tenantId ?? string.Empty,
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow
        };

        _db.BillsOfMaterials.Add(bom);
        await _db.SaveChangesAsync();

        _log.LogInformation("BOM created for item ID {ItemId} version {Version}", model.ItemId, model.Version);
        TempData["Success"] = "BOM created successfully. Add materials to complete the BOM.";
        return RedirectToAction(nameof(Details), new { id = bom.BomId });
    }

    // ── GET /BOM/Details/5 ──────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var bom = await _db.BillsOfMaterials
            .Include(b => b.Item)
            .Include(b => b.BomLines!)
                .ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(b => b.BomId == id);

        if (bom == null)
            return NotFound();

        await PopulateFinishedGoods();
        return View(bom);
    }

    // ── GET /BOM/Edit/5 ─────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var bom = await _db.BillsOfMaterials
            .Include(b => b.Item)
            .FirstOrDefaultAsync(b => b.BomId == id);

        if (bom == null)
            return NotFound();

        await PopulateFinishedGoods();

        var model = new BomFormViewModel
        {
            BomId = bom.BomId,
            ItemId = bom.ItemId,
            Version = bom.Version,
            BatchOutputQty = bom.BatchOutputQty,
            BatchOutputUom = bom.BatchOutputUom,
            EstMachineHours = bom.EstMachineHours,
            EstLaborHours = bom.EstLaborHours,
            IsActive = bom.IsActive,
            Notes = bom.Notes
        };

        return View(model);
    }

    // ── POST /BOM/Edit/5 ────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BomFormViewModel model)
    {
        if (id != model.BomId)
            return NotFound();

        var bom = await _db.BillsOfMaterials.FindAsync(id);
        if (bom == null)
            return NotFound();

        // Check for duplicate version (excluding current)
        var existingVersion = await _db.BillsOfMaterials
            .FirstOrDefaultAsync(b => b.BomId != id && b.ItemId == model.ItemId && b.Version == model.Version);
        
        if (existingVersion != null)
        {
            ModelState.AddModelError("Version", "Another BOM with this version already exists for this item.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateFinishedGoods();
            return View(model);
        }

        bom.ItemId          = model.ItemId;
        bom.Version         = model.Version;
        bom.BatchOutputQty  = model.BatchOutputQty;
        bom.BatchOutputUom  = model.BatchOutputUom;
        bom.EstMachineHours = model.EstMachineHours;
        bom.EstLaborHours   = model.EstLaborHours;
        bom.IsActive        = model.IsActive;
        bom.Notes           = model.Notes;
        bom.UpdatedAt       = DateTime.UtcNow;
        // TenantId is intentionally NOT updated — preserve the original owner

        _db.BillsOfMaterials.Update(bom);
        await _db.SaveChangesAsync();

        _log.LogInformation("BOM {BomId} updated", id);
        TempData["Success"] = "BOM updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── GET /BOM/Delete/5 ───────────────────────────────────
    public async Task<IActionResult> Delete(int id)
    {
        var bom = await _db.BillsOfMaterials
            .Include(b => b.Item)
            .Include(b => b.BomLines)
            .FirstOrDefaultAsync(b => b.BomId == id);

        if (bom == null)
            return NotFound();

        return View(bom);
    }

    // ── POST /BOM/Delete/5 ──────────────────────────────────
    [HttpPost("BOM/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bom = await _db.BillsOfMaterials
            .Include(b => b.BomLines)
            .FirstOrDefaultAsync(b => b.BomId == id);

        if (bom == null)
            return NotFound();

        // Check if any work orders use this BOM
        var woCount = await _db.WorkOrders.CountAsync(w => w.BomId == id);
        if (woCount > 0)
        {
            TempData["Error"] = $"Cannot delete BOM: {woCount} work order(s) depend on it.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Delete BOM lines first
        _db.BomLines.RemoveRange(bom.BomLines!);
        
        // Delete BOM
        _db.BillsOfMaterials.Remove(bom);
        await _db.SaveChangesAsync();

        _log.LogInformation("BOM {BomId} deleted", id);
        TempData["Success"] = "BOM deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /BOM/AddLine ────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLine(BomLineFormViewModel model)
    {
        if (model.BomId <= 0)
        {
            TempData["Error"] = "Invalid BOM reference.";
            return RedirectToAction(nameof(Index));
        }

        var bom = await _db.BillsOfMaterials
            .Include(b => b.BomLines)
            .FirstOrDefaultAsync(b => b.BomId == model.BomId);
        if (bom == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all required fields correctly.";
            TempData["ShowAddLineForm"] = "1";
            return RedirectToAction(nameof(Details), new { id = model.BomId });
        }

        var item = await _db.Items
            .Where(i => i.ItemId == model.ItemId)
            .Select(i => new { i.ItemId, i.ItemType, i.IsActive })
            .FirstOrDefaultAsync();

        if (item == null)
        {
            TempData["Error"] = "Selected material does not exist.";
            TempData["ShowAddLineForm"] = "1";
            return RedirectToAction(nameof(Details), new { id = model.BomId });
        }

        if (!item.IsActive || item.ItemType != "RawMaterial")
        {
            TempData["Error"] = "Only active raw materials can be added to the BOM.";
            TempData["ShowAddLineForm"] = "1";
            return RedirectToAction(nameof(Details), new { id = model.BomId });
        }

        var alreadyExists = bom.BomLines?.Any(l => l.ItemId == model.ItemId) == true;
        if (alreadyExists)
        {
            TempData["Error"] = "This material already exists in the BOM.";
            TempData["ShowAddLineForm"] = "1";
            return RedirectToAction(nameof(Details), new { id = model.BomId });
        }

        var bomLine = new BomLine
        {
            BomId = model.BomId,
            ItemId = model.ItemId,
            QtyPerBatch = model.Quantity,
            UnitOfMeasure = model.UnitOfMeasure,
            Notes = model.Notes
        };

        try
        {
            _db.BomLines.Add(bomLine);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _log.LogError(ex, "Failed to add BOM line for BOM {BomId}", model.BomId);
            TempData["Error"] = "Could not add material due to a data error. Please try again.";
            TempData["ShowAddLineForm"] = "1";
            return RedirectToAction(nameof(Details), new { id = model.BomId });
        }

        TempData["Success"] = "Material added to BOM.";
        return RedirectToAction(nameof(Details), new { id = model.BomId });
    }

    // ── POST /BOM/RemoveLine/5 ──────────────────────────────
    [HttpPost("BOM/RemoveLine/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLine(int id)
    {
        var bomLine = await _db.BomLines.FindAsync(id);
        if (bomLine == null)
            return NotFound();

        var bomId = bomLine.BomId;
        _db.BomLines.Remove(bomLine);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Material removed from BOM.";
        return RedirectToAction(nameof(Details), new { id = bomId });
    }

    // ── Helper: Populate finished goods dropdown ─────────────
    private async Task PopulateFinishedGoods()
    {
        ViewBag.FinishedGoods = new SelectList(
            await _db.Items
                .Where(i => i.ItemType == "FinishedGood" && i.IsActive)
                .OrderBy(i => i.ItemName)
                .ToListAsync(),
            "ItemId", "ItemName");

        ViewBag.RawMaterials = new SelectList(
            await _db.Items
                .Where(i => i.ItemType == "RawMaterial" && i.IsActive)
                .OrderBy(i => i.ItemName)
                .ToListAsync(),
            "ItemId", "ItemName");
    }
}
