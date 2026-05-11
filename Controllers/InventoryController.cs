using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.Models;
using SnackFlowMES.ViewModels;

namespace SnackFlowMES.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<InventoryController> _log;

    public InventoryController(
        ApplicationDbContext db,
        ILogger<InventoryController> log)
    {
        _db  = db;
        _log = log;
    }

    // ── GET /Inventory ───────────────────────────────────────
    public async Task<IActionResult> Index(string? type = null, string? q = null, string? lowstock = null, string? archived = null, int page = 1)
    {
        const int pageSize = 10;
        
        var baseQuery = _db.Items
            .Include(i => i.InventoryBalance)
            .AsQueryable();

        // Filter by archived status first
        var showArchived = archived == "true";
        if (showArchived)
        {
            baseQuery = baseQuery.Where(i => i.IsArchived);
        }
        else
        {
            baseQuery = baseQuery.Where(i => !i.IsArchived);
        }

        // compute counts for tabs (total, per type)
        ViewBag.TotalCount = await baseQuery.CountAsync();
        ViewBag.RawMaterialCount = await baseQuery.CountAsync(i => i.ItemType == "RawMaterial");
        ViewBag.PackagingCount = await baseQuery.CountAsync(i => i.ItemType == "Packaging");
        ViewBag.FinishedGoodCount = await baseQuery.CountAsync(i => i.ItemType == "FinishedGood");
        ViewBag.SemiFinishedCount = await baseQuery.CountAsync(i => i.ItemType == "SemiFinished");
        ViewBag.ArchivedCount = await _db.Items.CountAsync(i => i.IsArchived);
        ViewBag.ShowArchived = showArchived;

        var query = baseQuery;

        if (!string.IsNullOrEmpty(type))
            query = query.Where(i => i.ItemType == type);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var t = q.Trim().ToLower();
            query = query.Where(i => i.ItemName.ToLower().Contains(t)
                || i.ItemCode.ToLower().Contains(t)
                || (i.Category != null && i.Category.ToLower().Contains(t)));
        }

        if (!string.IsNullOrWhiteSpace(lowstock) && lowstock == "normal")
        {
            query = query.Where(i => i.ReorderPoint <= 0 || (i.InventoryBalance == null || i.InventoryBalance.QtyOnHand > i.ReorderPoint));
        }
        else if (!string.IsNullOrWhiteSpace(lowstock) && lowstock == "low")
        {
            query = query.Where(i => i.ReorderPoint > 0 && (i.InventoryBalance != null && i.InventoryBalance.QtyOnHand <= i.ReorderPoint));
        }

        // Get total count for pagination
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        // Ensure page is within valid range
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var items = await query
            .OrderBy(i => i.ItemName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.TypeFilter = type;
        ViewBag.Q = q ?? string.Empty;
        ViewBag.LowStock = string.IsNullOrWhiteSpace(lowstock) ? "all" : lowstock;
        ViewBag.Archived = archived ?? "false";
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.PageSize = pageSize;
        
        return View(items);
    }

    // ── GET /Inventory/Details/5 ─────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Items
            .Include(i => i.InventoryBalance)
            .Include(i => i.InventoryLedgers!)
                .ThenInclude(l => l.WorkOrder)
            .FirstOrDefaultAsync(i => i.ItemId == id);

        if (item == null)
            return NotFound();

        return View(item);
    }

    // ── GET /Inventory/Create ────────────────────────────────
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        PopulateDropdowns();
        return View(new ItemFormViewModel());
    }

    // ── POST /Inventory/Create ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            PopulateDropdowns();
            return View(model);
        }

        // Check for duplicate item code
        if (await _db.Items.AnyAsync(i => i.ItemCode == model.ItemCode))
        {
            ModelState.AddModelError("ItemCode", "Item code already exists.");
            PopulateDropdowns();
            return View(model);
        }

        var item = new Item
        {
            ItemCode      = model.ItemCode,
            ItemName      = model.ItemName,
            ItemType      = model.ItemType,
            UnitOfMeasure = model.UnitOfMeasure,
            Category      = model.Category,
            UnitCost      = model.UnitCost,
            ReorderPoint  = model.ReorderPoint,
            Description   = model.Description,
            IsActive      = true,
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow
        };

        _db.Items.Add(item);
        await _db.SaveChangesAsync();

        // Create initial inventory balance
        var balance = new InventoryBalance
        {
            ItemId      = item.ItemId,
            QtyOnHand   = 0,
            LastUpdated = DateTime.UtcNow
        };
        _db.InventoryBalances.Add(balance);
        await _db.SaveChangesAsync();

        _log.LogInformation("Item {ItemCode} created", item.ItemCode);
        TempData["Success"] = $"Item {item.ItemCode} created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Inventory/Edit/5 ────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null)
            return NotFound();

        var model = new ItemFormViewModel
        {
            ItemId        = item.ItemId,
            ItemCode      = item.ItemCode,
            ItemName      = item.ItemName,
            ItemType      = item.ItemType,
            UnitOfMeasure = item.UnitOfMeasure,
            Category      = item.Category,
            UnitCost      = item.UnitCost,
            ReorderPoint  = item.ReorderPoint,
            Description   = item.Description
        };

        PopulateDropdowns();
        return View(model);
    }

    // ── POST /Inventory/Edit/5 ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, ItemFormViewModel model)
    {
        if (id != model.ItemId)
            return NotFound();

        if (!ModelState.IsValid)
        {
            PopulateDropdowns();
            return View(model);
        }

        var item = await _db.Items.FindAsync(id);
        if (item == null)
            return NotFound();

        // Check for duplicate item code (excluding current item)
        if (await _db.Items.AnyAsync(i => i.ItemCode == model.ItemCode && i.ItemId != id))
        {
            ModelState.AddModelError("ItemCode", "Item code already exists.");
            PopulateDropdowns();
            return View(model);
        }

        item.ItemCode      = model.ItemCode;
        item.ItemName      = model.ItemName;
        item.ItemType      = model.ItemType;
        item.UnitOfMeasure = model.UnitOfMeasure;
        item.Category      = model.Category;
        item.UnitCost      = model.UnitCost;
        item.ReorderPoint  = model.ReorderPoint;
        item.Description   = model.Description;
        item.UpdatedAt     = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _log.LogInformation("Item {ItemCode} updated", item.ItemCode);
        TempData["Success"] = $"Item {item.ItemCode} updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Inventory/UpdateQuantity ───────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateQuantity(int id, decimal quantity, string transactionType)
    {
        var item = await _db.Items
            .Include(i => i.InventoryBalance)
            .FirstOrDefaultAsync(i => i.ItemId == id);

        if (item == null)
            return NotFound();

        var balance = item.InventoryBalance ?? new InventoryBalance
        {
            ItemId      = item.ItemId,
            QtyOnHand   = 0,
            LastUpdated = DateTime.UtcNow
        };

        if (item.InventoryBalance == null)
            _db.InventoryBalances.Add(balance);

        // Update quantity based on transaction type
        if (transactionType == "Add")
            balance.QtyOnHand += quantity;
        else if (transactionType == "Remove")
            balance.QtyOnHand -= quantity;
        else if (transactionType == "Set")
            balance.QtyOnHand = quantity;

        balance.LastUpdated = DateTime.UtcNow;

        // Create ledger entry
        var ledger = new InventoryLedger
        {
            ItemId       = item.ItemId,
            MovementType = transactionType,
            Qty          = quantity,
            BalanceAfter = balance.QtyOnHand,
            PostedAt     = DateTime.UtcNow,
            Notes        = $"Manual adjustment: {transactionType}"
        };
        _db.InventoryLedgers.Add(ledger);

        await _db.SaveChangesAsync();

        _log.LogInformation("Inventory updated for {ItemCode}: {Type} {Qty}", item.ItemCode, transactionType, quantity);
        TempData["Success"] = $"Inventory updated for {item.ItemCode}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── POST /Inventory/Archive ──────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Archive(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null)
            return NotFound();

        if (item.IsArchived)
        {
            TempData["Error"] = $"Item {item.ItemCode} is already archived.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Check if item is used in any active BOMs
        var activeBomCount = await _db.BomLines
            .Include(bl => bl.Bom)
            .CountAsync(bl => bl.ItemId == id && bl.Bom!.IsActive);

        if (activeBomCount > 0)
        {
            TempData["Error"] = $"Cannot archive item {item.ItemCode}: It is used in {activeBomCount} active BOM(s). Please deactivate or remove from BOMs first.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Check if item is used in any active/in-progress work orders
        var activeWorkOrderCount = await _db.WorkOrders
            .CountAsync(w => w.ItemId == id && (w.Status == "Released" || w.Status == "InProgress"));

        if (activeWorkOrderCount > 0)
        {
            TempData["Error"] = $"Cannot archive item {item.ItemCode}: It is used in {activeWorkOrderCount} active work order(s). Please complete or cancel them first.";
            return RedirectToAction(nameof(Details), new { id });
        }

        item.IsArchived = true;
        item.IsActive = false; // Also deactivate when archiving
        item.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _log.LogInformation("Item {ItemCode} archived", item.ItemCode);
        TempData["Success"] = $"Item {item.ItemCode} has been archived. It can now be permanently deleted if needed.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Inventory/Unarchive ────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Unarchive(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null)
            return NotFound();

        if (!item.IsArchived)
        {
            TempData["Error"] = $"Item {item.ItemCode} is not archived.";
            return RedirectToAction(nameof(Details), new { id });
        }

        item.IsArchived = false;
        item.IsActive = true; // Reactivate when unarchiving
        item.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _log.LogInformation("Item {ItemCode} unarchived", item.ItemCode);
        TempData["Success"] = $"Item {item.ItemCode} has been restored from archive.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── POST /Inventory/Delete ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Items
            .Include(i => i.InventoryBalance)
            .Include(i => i.BomLines)
            .Include(i => i.InventoryLedgers)
            .FirstOrDefaultAsync(i => i.ItemId == id);

        if (item == null)
            return NotFound();

        // Safety check: Only allow deletion of archived items
        if (!item.IsArchived)
        {
            TempData["Error"] = $"Cannot delete item {item.ItemCode}. Only archived items can be permanently deleted. Please archive it first.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var itemCode = item.ItemCode;

        try
        {
            // Delete related records first
            if (item.InventoryBalance != null)
            {
                _db.InventoryBalances.Remove(item.InventoryBalance);
            }

            if (item.InventoryLedgers != null && item.InventoryLedgers.Any())
            {
                _db.InventoryLedgers.RemoveRange(item.InventoryLedgers);
            }

            if (item.BomLines != null && item.BomLines.Any())
            {
                _db.BomLines.RemoveRange(item.BomLines);
            }

            // Delete the item
            _db.Items.Remove(item);
            await _db.SaveChangesAsync();

            _log.LogInformation("Item {ItemCode} permanently deleted", itemCode);
            TempData["Success"] = $"Item {itemCode} has been permanently deleted from the system.";
            return RedirectToAction(nameof(Index), new { archived = "true" });
        }
        catch (DbUpdateException ex)
        {
            _log.LogWarning(ex, "Failed to delete item {ItemCode} due to foreign key constraint", itemCode);
            TempData["Error"] = $"Cannot delete item {itemCode} because it has associated records in the system (e.g., work orders, production plans). These records must be deleted first.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error deleting item {ItemCode}", itemCode);
            TempData["Error"] = $"An unexpected error occurred while deleting item {itemCode}. Please try again or contact support.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private void PopulateDropdowns()
    {
        ViewBag.ItemTypes = new SelectList(new[]
        {
            "RawMaterial", "Packaging", "FinishedGood", "SemiFinished"
        });

        ViewBag.UnitOfMeasures = new SelectList(new[]
        {
            "kg", "g", "L", "mL", "pcs", "bag", "box", "pack"
        });
    }
}
