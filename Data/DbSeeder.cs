using Microsoft.AspNetCore.Identity;
using SnackFlowMES.Models;

namespace SnackFlowMES.Data;

/// <summary>
/// Seeds roles, default users per role, sample items, and initial inventory balances.
/// Safe to call on every startup — skips anything already present.
/// </summary>
public static class DbSeeder
{
    // ── Role names (match Use Case Diagram exactly) ──────────
    public const string Admin    = "Admin";
    public const string Planner  = "Planner";
    public const string Operator = "Operator";
    public const string QC       = "QC";
    public const string Manager  = "Manager";

    public static async Task SeedAsync(
        ApplicationDbContext      db,
        UserManager<ApplicationUser>  userManager,
        RoleManager<IdentityRole>     roleManager)
    {
        // ── 1. Roles ─────────────────────────────────────────
        string[] roles = [Admin, Planner, Operator, QC, Manager];
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // ── 2. Default users (one per role) ──────────────────
        await EnsureUser(userManager, new ApplicationUser
        {
            UserName = "admin@snackflow.local",
            Email    = "admin@snackflow.local",
            FullName = "System Administrator",
            EmailConfirmed = true,
            IsActive = true
        }, "Admin@1234!", Admin);

        await EnsureUser(userManager, new ApplicationUser
        {
            UserName = "planner@snackflow.local",
            Email    = "planner@snackflow.local",
            FullName = "Production Planner",
            EmailConfirmed = true,
            IsActive = true
        }, "Planner@1234!", Planner);

        await EnsureUser(userManager, new ApplicationUser
        {
            UserName = "operator@snackflow.local",
            Email    = "operator@snackflow.local",
            FullName = "Floor Operator",
            EmailConfirmed = true,
            IsActive = true
        }, "Operator@1234!", Operator);

        await EnsureUser(userManager, new ApplicationUser
        {
            UserName = "qc@snackflow.local",
            Email    = "qc@snackflow.local",
            FullName = "QC Inspector",
            EmailConfirmed = true,
            IsActive = true
        }, "QcUser@1234!", QC);

        await EnsureUser(userManager, new ApplicationUser
        {
            UserName = "manager@snackflow.local",
            Email    = "manager@snackflow.local",
            FullName = "Plant Manager",
            EmailConfirmed = true,
            IsActive = true
        }, "Manager@1234!", Manager);

        // ── 3. Sample Items ───────────────────────────────────
        if (!db.Items.Any())
        {
            var items = new List<Item>
            {
                // Raw Materials — Ingredients
                new() { ItemCode="RM-001", ItemName="Potato Starch",       ItemType="RawMaterial", UnitOfMeasure="kg",  Category="Ingredient",  UnitCost=45.00m,  ReorderPoint=100 },
                new() { ItemCode="RM-002", ItemName="Vegetable Oil",        ItemType="RawMaterial", UnitOfMeasure="L",   Category="Ingredient",  UnitCost=62.00m,  ReorderPoint=50  },
                new() { ItemCode="RM-003", ItemName="Fine Salt",            ItemType="RawMaterial", UnitOfMeasure="kg",  Category="Ingredient",  UnitCost=12.00m,  ReorderPoint=30  },
                new() { ItemCode="RM-004", ItemName="Wheat Flour",          ItemType="RawMaterial", UnitOfMeasure="kg",  Category="Ingredient",  UnitCost=38.00m,  ReorderPoint=200 },
                new() { ItemCode="RM-005", ItemName="Cheese Powder",        ItemType="RawMaterial", UnitOfMeasure="kg",  Category="Flavoring",   UnitCost=185.00m, ReorderPoint=20  },
                new() { ItemCode="RM-006", ItemName="Baking Soda",          ItemType="RawMaterial", UnitOfMeasure="kg",  Category="Ingredient",  UnitCost=25.00m,  ReorderPoint=15  },
                new() { ItemCode="RM-007", ItemName="Butter",               ItemType="RawMaterial", UnitOfMeasure="kg",  Category="Ingredient",  UnitCost=220.00m, ReorderPoint=20  },
                new() { ItemCode="RM-008", ItemName="Sugar",                ItemType="RawMaterial", UnitOfMeasure="kg",  Category="Ingredient",  UnitCost=55.00m,  ReorderPoint=50  },
                new() { ItemCode="RM-009", ItemName="Onion Powder",         ItemType="RawMaterial", UnitOfMeasure="kg",  Category="Flavoring",   UnitCost=140.00m, ReorderPoint=10  },
                new() { ItemCode="RM-010", ItemName="Paprika",              ItemType="RawMaterial", UnitOfMeasure="kg",  Category="Flavoring",   UnitCost=160.00m, ReorderPoint=10  },
                // Packaging
                new() { ItemCode="PK-001", ItemName="Foil Bag 50g",         ItemType="Packaging",   UnitOfMeasure="pcs", Category="Packaging",   UnitCost=2.50m,   ReorderPoint=500 },
                new() { ItemCode="PK-002", ItemName="Foil Bag 100g",        ItemType="Packaging",   UnitOfMeasure="pcs", Category="Packaging",   UnitCost=3.20m,   ReorderPoint=500 },
                new() { ItemCode="PK-003", ItemName="Cardboard Box 24-pack",ItemType="Packaging",   UnitOfMeasure="pcs", Category="Packaging",   UnitCost=18.00m,  ReorderPoint=100 },
                new() { ItemCode="PK-004", ItemName="Shrink Wrap Roll",     ItemType="Packaging",   UnitOfMeasure="roll",Category="Packaging",   UnitCost=95.00m,  ReorderPoint=10  },
                // Finished Goods
                new() { ItemCode="FG-001", ItemName="Sea Salt Chips 50g",   ItemType="FinishedGood",UnitOfMeasure="bag", Category="Chips",       UnitCost=0m,      ReorderPoint=0   },
                new() { ItemCode="FG-002", ItemName="Cheese Crackers 100g", ItemType="FinishedGood",UnitOfMeasure="bag", Category="Crackers",    UnitCost=0m,      ReorderPoint=0   },
                new() { ItemCode="FG-003", ItemName="Baked Snack Bites 50g",ItemType="FinishedGood",UnitOfMeasure="bag", Category="BakedSnacks", UnitCost=0m,      ReorderPoint=0   },
                new() { ItemCode="FG-004", ItemName="BBQ Chips 100g",       ItemType="FinishedGood",UnitOfMeasure="bag", Category="Chips",       UnitCost=0m,      ReorderPoint=0   },
            };
            db.Items.AddRange(items);
            await db.SaveChangesAsync();

            // ── 4. Initial Inventory Balances (raw + packaging only) ──
            var stockableItems = db.Items
                .Where(i => i.ItemType == "RawMaterial" || i.ItemType == "Packaging")
                .Select(i => i.ItemId)
                .ToList();

            db.InventoryBalances.AddRange(stockableItems.Select(id => new InventoryBalance
            {
                ItemId      = id,
                QtyOnHand   = 0,
                QtyReserved = 0,
                LastUpdated = DateTime.UtcNow
            }));
            await db.SaveChangesAsync();
        }
    }

    // ── Helper: create user + assign role if not exists ──────
    private static async Task EnsureUser(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string password,
        string role)
    {
        if (await userManager.FindByEmailAsync(user.Email!) is null)
        {
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }
}
