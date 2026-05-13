using System.Text.Json;
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

        // ── Load notifications for dashboard ─────────────────
        if (user != null)
        {
            var tenantId = user.TenantId;
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                tenantId = roles.Contains("Admin") ? user.Id : user.Id;
            }

            // Get user-specific notifications
            var userNotifications = await _db.Notifications
                .Where(n => n.RecipientUserId == user.Id && !n.IsRead && (string.IsNullOrEmpty(n.TenantId) || n.TenantId == tenantId))
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Get role-based notifications - filter by TenantId
            var roleNotifications = await _db.Notifications
                .Where(n => n.RecipientRole == role && !n.IsRead && (string.IsNullOrEmpty(n.TenantId) || n.TenantId == tenantId))
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Combine and pass to view
            ViewBag.Notifications = userNotifications
                .Concat(roleNotifications)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        // ── Load QC-specific data for QC dashboard ───────────
        if (role == "QC")
        {
            // Get QC inspection counts
            var today = DateTime.Today;
            ViewBag.InspectionsToday = await _db.QcResults
                .CountAsync(q => q.InspectedAt.Date == today);
            
            ViewBag.TotalInspections = await _db.QcResults.CountAsync();
            ViewBag.PassedInspections = await _db.QcResults
                .CountAsync(q => q.Result == "Pass");
            
            // Get work orders that have QC results
            ViewBag.QcResults = await _db.QcResults
                .Select(q => q.WorkOrderId)
                .Distinct()
                .ToListAsync();
        }

        // ── Operator dashboard: work orders by calendar day (current status) ──
        if (role == "Operator")
        {
            var trendPoints = await GetOperatorDailyStatusTrendAsync();
            ViewBag.OperatorDailyStatusTrend = JsonSerializer.Serialize(
                trendPoints,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        // ── Load downtime data for Admin and Manager ─────────
        if (role == "Admin" || role == "Manager")
        {
            ViewBag.OpenDowntimeCount = await _db.DowntimeReports
                .CountAsync(d => d.Status == "Open" && d.OrganizationId == (user.TenantId ?? user.Id));
        }

        // ── Prepare chart data for Admin and Manager ─────────
        if (role == "Admin" || role == "Manager")
        {
            // Production trend (last 7 days)
            var sevenDaysAgo = DateTime.Today.AddDays(-6);
            var productionTrend = await _db.WorkOrders
                .Where(w => w.CreatedAt >= sevenDaysAgo)
                .GroupBy(w => w.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Fill in missing dates with 0
            var trendData = new List<object>();
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Today.AddDays(-6 + i);
                var count = productionTrend.FirstOrDefault(p => p.Date == date)?.Count ?? 0;
                trendData.Add(new { date = date.ToString("MMM dd"), count });
            }
            ViewBag.ProductionTrend = System.Text.Json.JsonSerializer.Serialize(trendData);

            // Downtime by reason (if any downtime exists)
            var downtimeByReason = await _db.DowntimeReports
                .Where(d => d.OrganizationId == (user.TenantId ?? user.Id))
                .GroupBy(d => d.Reason)
                .Select(g => new { reason = g.Key, count = g.Count(), duration = g.Sum(d => d.DurationMinutes) })
                .OrderByDescending(x => x.duration)
                .Take(5)
                .ToListAsync();
            ViewBag.DowntimeByReason = System.Text.Json.JsonSerializer.Serialize(downtimeByReason);
        }

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

    /// <summary>JSON for operator dashboard chart; polled for near real-time updates.</summary>
    [HttpGet]
    public async Task<IActionResult> OperatorChartData()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var roles = await _users.GetRolesAsync(user);
        if (!roles.Contains("Operator"))
            return Forbid();

        var trend = await GetOperatorDailyStatusTrendAsync();
        return Json(trend);
    }

    private static int SumStatusCounts(IEnumerable<(string Status, int Count)> rows, string status) =>
        rows.Where(r => r.Status == status).Sum(r => r.Count);

    private async Task<List<OperatorDailyPoint>> GetOperatorDailyStatusTrendAsync()
    {
        var trendStart = DateTime.Today.AddDays(-13);
        var bucketRows = await _db.WorkOrders
            .Where(w => w.CreatedAt >= trendStart)
            .GroupBy(w => new { Day = w.CreatedAt.Date, w.Status })
            .Select(g => new { g.Key.Day, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        var trendPoints = new List<OperatorDailyPoint>();
        for (var i = 0; i < 14; i++)
        {
            var day = trendStart.AddDays(i);
            var rows = bucketRows
                .Where(b => b.Day == day)
                .Select(b => (b.Status, b.Count));

            trendPoints.Add(new OperatorDailyPoint
            {
                Date = day.ToString("MMM dd"),
                Draft = SumStatusCounts(rows, "Draft"),
                Released = SumStatusCounts(rows, "Released"),
                InProgress = SumStatusCounts(rows, "InProgress"),
                Completed = SumStatusCounts(rows, "Completed"),
                Cancelled = SumStatusCounts(rows, "Cancelled")
            });
        }

        return trendPoints;
    }

    private sealed class OperatorDailyPoint
    {
        public string Date { get; init; } = "";
        public int Draft { get; init; }
        public int Released { get; init; }
        public int InProgress { get; init; }
        public int Completed { get; init; }
        public int Cancelled { get; init; }
    }
}
