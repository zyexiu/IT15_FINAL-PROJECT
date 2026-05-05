using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.ViewModels;

namespace SnackFlowMES.Controllers;

[Authorize]
public class ReportController : Controller
{
    private readonly ApplicationDbContext _db;

    public ReportController(ApplicationDbContext db)
    {
        _db = db;
    }

    // ── GET /Report ──────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var vm = new ReportViewModel
        {
            // Work Order Statistics
            TotalWorkOrders     = await _db.WorkOrders.CountAsync(),
            DraftWorkOrders     = await _db.WorkOrders.CountAsync(w => w.Status == "Draft"),
            ReleasedWorkOrders  = await _db.WorkOrders.CountAsync(w => w.Status == "Released"),
            InProgressWorkOrders = await _db.WorkOrders.CountAsync(w => w.Status == "InProgress"),
            CompletedWorkOrders = await _db.WorkOrders.CountAsync(w => w.Status == "Completed"),
            CancelledWorkOrders = await _db.WorkOrders.CountAsync(w => w.Status == "Cancelled"),

            // Production Statistics
            TotalPlannedQty = await _db.WorkOrders
                .Where(w => w.Status == "Completed")
                .SumAsync(w => w.PlannedQty),
            TotalActualQty = await _db.WorkOrders
                .Where(w => w.Status == "Completed")
                .SumAsync(w => w.ActualQty),

            // Inventory Statistics
            TotalItems = await _db.Items.CountAsync(i => i.IsActive),
            RawMaterialsCount = await _db.Items.CountAsync(i => i.ItemType == "RawMaterial" && i.IsActive),
            PackagingCount = await _db.Items.CountAsync(i => i.ItemType == "Packaging" && i.IsActive),
            FinishedGoodsCount = await _db.Items.CountAsync(i => i.ItemType == "FinishedGood" && i.IsActive),
            LowStockItems = await _db.InventoryBalances
                .Include(b => b.Item)
                .CountAsync(b => b.QtyOnHand <= b.Item!.ReorderPoint && b.Item.ReorderPoint > 0),

            // Production Plans
            TotalProductionPlans = await _db.ProductionPlans.CountAsync(),
            ActiveProductionPlans = await _db.ProductionPlans
                .CountAsync(p => p.Status == "InProgress" || p.Status == "Approved"),

            // Recent Activity
            RecentWorkOrders = await _db.WorkOrders
                .Include(w => w.Item)
                .OrderByDescending(w => w.CreatedAt)
                .Take(10)
                .Select(w => new WorkOrderSummary
                {
                    WorkOrderId    = w.WorkOrderId,
                    WoNumber       = w.WoNumber,
                    ItemName       = w.Item != null ? w.Item.ItemName : "—",
                    PlannedQty     = w.PlannedQty,
                    ActualQty      = w.ActualQty,
                    UnitOfMeasure  = w.UnitOfMeasure,
                    Status         = w.Status,
                    ScheduledStart = w.ScheduledStart,
                    ProductionLine = w.ProductionLine
                })
                .ToListAsync()
        };

        // Calculate completion rate
        if (vm.TotalWorkOrders > 0)
        {
            vm.CompletionRate = (decimal)vm.CompletedWorkOrders / vm.TotalWorkOrders * 100;
        }

        // Calculate yield rate
        if (vm.TotalPlannedQty > 0)
        {
            vm.YieldRate = vm.TotalActualQty / vm.TotalPlannedQty * 100;
        }

        return View(vm);
    }
}
