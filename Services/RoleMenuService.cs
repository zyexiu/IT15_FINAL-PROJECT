using SnackFlowMES.Models;

namespace SnackFlowMES.Services;

/// <summary>
/// Service for managing role-based menu items and access control
/// </summary>
public class RoleMenuService
{
    /// <summary>
    /// Get menu items for a specific role
    /// </summary>
    public static List<MenuItem> GetMenuForRole(string role)
    {
        return role switch
        {
            "Admin" => GetAdminMenu(),
            "Planner" => GetPlannerMenu(),
            "Operator" => GetOperatorMenu(),
            "QC" => GetQCMenu(),
            "Manager" => GetManagerMenu(),
            _ => GetDefaultMenu()
        };
    }

    /// <summary>
    /// Check if a role has access to a specific controller/action
    /// </summary>
    public static bool HasAccess(string role, string controller, string action = "Index")
    {
        var menu = GetMenuForRole(role);
        return menu.Any(m => 
            m.Controller.Equals(controller, StringComparison.OrdinalIgnoreCase) ||
            m.SubItems?.Any(s => s.Controller.Equals(controller, StringComparison.OrdinalIgnoreCase)) == true
        );
    }

    // ── Admin: Full Access ───────────────────────────────────
    private static List<MenuItem> GetAdminMenu()
    {
        return new List<MenuItem>
        {
            new MenuItem
            {
                Title = "Dashboard",
                Controller = "Dashboard",
                Action = "Index",
                Icon = "dashboard",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Work Orders",
                Controller = "WorkOrder",
                Action = "Index",
                Icon = "file-text",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Production Planning",
                Controller = "ProductionPlan",
                Action = "Index",
                Icon = "calendar",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Bill of Materials",
                Controller = "Bom",
                Action = "Index",
                Icon = "layers",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Inventory",
                Controller = "Inventory",
                Action = "Index",
                Icon = "package",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Reports",
                Controller = "Report",
                Action = "Index",
                Icon = "bar-chart",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Users",
                Controller = "Users",
                Action = "Index",
                Icon = "users",
                Section = "ADMINISTRATION"
            },
            new MenuItem
            {
                Title = "Settings",
                Controller = "Settings",
                Action = "Index",
                Icon = "settings",
                Section = "ADMINISTRATION"
            }
        };
    }

    // ── Planner: Production Planning & Work Orders ───────────
    private static List<MenuItem> GetPlannerMenu()
    {
        return new List<MenuItem>
        {
            new MenuItem
            {
                Title = "Dashboard",
                Controller = "Dashboard",
                Action = "Index",
                Icon = "dashboard",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Production Planning",
                Controller = "ProductionPlan",
                Action = "Index",
                Icon = "calendar",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Bill of Materials",
                Controller = "Bom",
                Action = "Index",
                Icon = "layers",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Work Orders",
                Controller = "WorkOrder",
                Action = "Index",
                Icon = "file-text",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Reports",
                Controller = "Report",
                Action = "Index",
                Icon = "bar-chart",
                Section = "MAIN MENU"
            }
        };
    }

    // ── Operator: View & Update Work Orders ──────────────────
    private static List<MenuItem> GetOperatorMenu()
    {
        return new List<MenuItem>
        {
            new MenuItem
            {
                Title = "Dashboard",
                Controller = "Dashboard",
                Action = "Index",
                Icon = "dashboard",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "My Work Orders",
                Controller = "WorkOrder",
                Action = "Index",
                Icon = "file-text",
                Section = "MAIN MENU"
            }
        };
    }

    // ── QC: Quality Control ───────────────────────────────────
    private static List<MenuItem> GetQCMenu()
    {
        return new List<MenuItem>
        {
            new MenuItem
            {
                Title = "Dashboard",
                Controller = "Dashboard",
                Action = "Index",
                Icon = "dashboard",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Quality Control",
                Controller = "WorkOrder",
                Action = "Index",
                Icon = "check-circle",
                Section = "MAIN MENU"
            }
        };
    }

    // ── Manager: Reports Only (Read-Only) ────────────────────
    private static List<MenuItem> GetManagerMenu()
    {
        return new List<MenuItem>
        {
            new MenuItem
            {
                Title = "Dashboard",
                Controller = "Dashboard",
                Action = "Index",
                Icon = "dashboard",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Reports",
                Controller = "Report",
                Action = "Index",
                Icon = "bar-chart",
                Section = "MAIN MENU"
            },
            new MenuItem
            {
                Title = "Production Overview",
                Controller = "WorkOrder",
                Action = "Index",
                Icon = "activity",
                Section = "MAIN MENU"
            }
        };
    }

    // ── Default: Dashboard Only ──────────────────────────────
    private static List<MenuItem> GetDefaultMenu()
    {
        return new List<MenuItem>
        {
            new MenuItem
            {
                Title = "Dashboard",
                Controller = "Dashboard",
                Action = "Index",
                Icon = "dashboard",
                Section = "MAIN MENU"
            }
        };
    }
}

/// <summary>
/// Menu item model
/// </summary>
public class MenuItem
{
    public string Title { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = "Index";
    public string Icon { get; set; } = "circle";
    public string Section { get; set; } = "MAIN MENU";
    public List<MenuItem>? SubItems { get; set; }
    public string? Badge { get; set; }
    public string? BadgeColor { get; set; }
}
