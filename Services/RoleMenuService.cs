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

    // ── Admin: System Administration Focus ──────────────────
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
                Section = "OVERVIEW"
            },
            new MenuItem
            {
                Title = "User Management",
                Controller = "Users",
                Action = "Index",
                Icon = "users",
                Section = "ADMINISTRATION"
            },
            new MenuItem
            {
                Title = "Work Orders",
                Controller = "WorkOrder",
                Action = "Index",
                Icon = "file-text",
                Section = "ADMINISTRATION"
            },
            new MenuItem
            {
                Title = "Material Request",
                Controller = "MaterialRequest",
                Action = "Index",
                Icon = "shopping-cart",
                Section = "ADMINISTRATION"
            },
            new MenuItem
            {
                Title = "Downtime Reports",
                Controller = "Downtime",
                Action = "Index",
                Icon = "alert-circle",
                Section = "ADMINISTRATION"
            },
            new MenuItem
            {
                Title = "Inventory Master",
                Controller = "Inventory",
                Action = "Index",
                Icon = "package",
                Section = "ADMINISTRATION"
            },
            new MenuItem
            {
                Title = "Reports & Analytics",
                Controller = "Report",
                Action = "Index",
                Icon = "bar-chart",
                Section = "ADMINISTRATION"
            },
            new MenuItem
            {
                Title = "System Settings",
                Controller = "Settings",
                Action = "Index",
                Icon = "settings",
                Section = "ADMINISTRATION"
            }
        };
    }

    // ── Planner: Production Planning & Scheduling ────────────
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
                Section = "OVERVIEW"
            },
            new MenuItem
            {
                Title = "Production Planning",
                Controller = "ProductionPlan",
                Action = "Index",
                Icon = "calendar",
                Section = "PLANNING"
            },
            new MenuItem
            {
                Title = "Work Orders",
                Controller = "WorkOrder",
                Action = "Index",
                Icon = "file-text",
                Section = "PLANNING"
            },
            new MenuItem
            {
                Title = "Bill of Materials",
                Controller = "Bom",
                Action = "Index",
                Icon = "layers",
                Section = "PLANNING"
            },
            new MenuItem
            {
                Title = "Material Request",
                Controller = "MaterialRequest",
                Action = "Index",
                Icon = "shopping-cart",
                Section = "PLANNING"
            },
            new MenuItem
            {
                Title = "Inventory",
                Controller = "Inventory",
                Action = "Index",
                Icon = "package",
                Section = "RESOURCES"
            },
            new MenuItem
            {
                Title = "Reports",
                Controller = "Report",
                Action = "Index",
                Icon = "bar-chart",
                Section = "RESOURCES"
            }
        };
    }

    // ── Operator: Production Floor Execution ─────────────────
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
                Section = "OVERVIEW"
            },
            new MenuItem
            {
                Title = "My Work Orders",
                Controller = "WorkOrder",
                Action = "Index",
                Icon = "file-text",
                Section = "PRODUCTION"
            },
            new MenuItem
            {
                Title = "Report Downtime",
                Controller = "Downtime",
                Action = "Create",
                Icon = "alert-circle",
                Section = "PRODUCTION"
            }
        };
    }

    // ── QC: Quality Control & Inspection ─────────────────────
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
                Section = "OVERVIEW"
            },
            new MenuItem
            {
                Title = "Quality Inspection",
                Controller = "WorkOrder",
                Action = "Index",
                Icon = "check-circle",
                Section = "QUALITY CONTROL"
            },
            new MenuItem
            {
                Title = "Inspection History",
                Controller = "Qc",
                Action = "Index",
                Icon = "file-text",
                Section = "QUALITY CONTROL"
            }
        };
    }

    // ── Manager: Business Analytics & Monitoring ─────────────
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
                Section = "OVERVIEW"
            },
            new MenuItem
            {
                Title = "Production Overview",
                Controller = "WorkOrder",
                Action = "Index",
                Icon = "activity",
                Section = "MONITORING"
            },
            new MenuItem
            {
                Title = "Downtime Reports",
                Controller = "Downtime",
                Action = "Index",
                Icon = "alert-circle",
                Section = "MONITORING"
            },
            new MenuItem
            {
                Title = "Production Planning",
                Controller = "ProductionPlan",
                Action = "Index",
                Icon = "calendar",
                Section = "MONITORING"
            },
            new MenuItem
            {
                Title = "Inventory Status",
                Controller = "Inventory",
                Action = "Index",
                Icon = "package",
                Section = "MONITORING"
            },
            new MenuItem
            {
                Title = "Reports & Analytics",
                Controller = "Report",
                Action = "Index",
                Icon = "bar-chart",
                Section = "ANALYTICS"
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
