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
    public async Task<IActionResult> Index(string? type = null, string? q = null, string? lowstock = null)
    {
        var baseQuery = _db.Items
            .Include(i => i.InventoryBalance)
            .AsQueryable();

        // compute counts for tabs (total, per type)
        ViewBag.TotalCount = await baseQuery.CountAsync();
        ViewBag.RawMaterialCount = await baseQuery.CountAsync(i => i.ItemType == "RawMaterial");
        ViewBag.PackagingCount = await baseQuery.CountAsync(i => i.ItemType == "Packaging");
        ViewBag.FinishedGoodCount = await baseQuery.CountAsync(i => i.ItemType == "FinishedGood");
        ViewBag.SemiFinishedCount = await baseQuery.CountAsync(i => i.ItemType == "SemiFinished");

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

        var items = await query
            .OrderBy(i => i.ItemName)
            .ToListAsync();

        ViewBag.TypeFilter = type;
        ViewBag.Q = q ?? string.Empty;
        ViewBag.LowStock = string.IsNullOrWhiteSpace(lowstock) ? "all" : lowstock;
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
    [Authorize(Roles = "Admin,Planner")]
    public IActionResult Create()
    {
        PopulateDropdowns();
        return View(new ItemFormViewModel());
    }

    // ── POST /Inventory/Create ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Planner")]
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
    [Authorize(Roles = "Admin,Planner")]
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
    [Authorize(Roles = "Admin,Planner")]
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
    [Authorize(Roles = "Admin,Planner")]
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
