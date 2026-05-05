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
    public async Task<IActionResult> Index()
    {
        var plans = await _db.ProductionPlans
            .Include(p => p.CreatedBy)
            .Include(p => p.PlanLines!)
                .ThenInclude(l => l.Item)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

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
}
