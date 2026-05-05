using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnackFlowMES.Data;
using SnackFlowMES.Models;
using SnackFlowMES.ViewModels;

namespace SnackFlowMES.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext          _db;
    private readonly UserManager<ApplicationUser>  _users;

    public DashboardController(
        ApplicationDbContext         db,
        UserManager<ApplicationUser> users)
    {
        _db    = db;
        _users = users;
    }

    // ── GET /Dashboard ───────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var user = await _users.GetUserAsync(User);
        var roles = user != null ? await _users.GetRolesAsync(user) : new List<string>();
        var role = roles.FirstOrDefault() ?? "User";

        var vm = new DashboardViewModel
        {
            // ── KPI counts ───────────────────────────────────
            TotalWorkOrders     = await _db.WorkOrders.CountAsync(),
            ActiveProduction    = await _db.WorkOrders
                                      .CountAsync(w => w.Status == "InProgress" || w.Status == "Released"),
            CompletedOrders     = await _db.WorkOrders
                                      .CountAsync(w => w.Status == "Completed"),
            TotalItems          = await _db.Items.CountAsync(),
            LowStockCount       = await _db.InventoryBalances
                                      .Include(b => b.Item)
                                      .CountAsync(b => b.QtyOnHand <= b.Item!.ReorderPoint && b.Item.ReorderPoint > 0),

            // ── Recent work orders (last 8) ───────────────────
            RecentWorkOrders    = await _db.WorkOrders
                                      .Include(w => w.Item)
                                      .OrderByDescending(w => w.CreatedAt)
                                      .Take(8)
                                      .Select(w => new WorkOrderSummary
                                      {
                                          WorkOrderId  = w.WorkOrderId,
                                          WoNumber     = w.WoNumber,
                                          ItemName     = w.Item != null ? w.Item.ItemName : "—",
                                          PlannedQty   = w.PlannedQty,
                                          ActualQty    = w.ActualQty,
                                          UnitOfMeasure = w.UnitOfMeasure,
                                          Status       = w.Status,
                                          ScheduledStart = w.ScheduledStart,
                                          ProductionLine = w.ProductionLine
                                      })
                                      .ToListAsync(),

            // ── Status breakdown for mini chart ──────────────
            DraftCount      = await _db.WorkOrders.CountAsync(w => w.Status == "Draft"),
            ReleasedCount   = await _db.WorkOrders.CountAsync(w => w.Status == "Released"),
            InProgressCount = await _db.WorkOrders.CountAsync(w => w.Status == "InProgress"),
            CompletedCount  = await _db.WorkOrders.CountAsync(w => w.Status == "Completed"),
            CancelledCount  = await _db.WorkOrders.CountAsync(w => w.Status == "Cancelled"),

            // ── Logged-in user info ───────────────────────────
            CurrentUserName = User.Identity?.Name ?? "User",
            CurrentUserRole = role
        };

        // Return role-specific view if it exists, otherwise default
        var viewName = role switch
        {
            "Admin" => "IndexAdmin",
            "Planner" => "IndexPlanner",
            "Operator" => "IndexOperator",
            "QC" => "IndexQC",
            "Manager" => "IndexManager",
            _ => "Index"
        };

        // Check if role-specific view exists, fallback to default
        if (System.IO.File.Exists(Path.Combine(
            Directory.GetCurrentDirectory(), 
            "Views", "Dashboard", $"{viewName}.cshtml")))
        {
            return View(viewName, vm);
        }

        return View(vm);
    }
}
