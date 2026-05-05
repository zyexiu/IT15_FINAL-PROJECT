using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SnackFlowMES.Models;

/// <summary>
/// Extends ASP.NET Core Identity user with SnackFlow-specific fields.
/// Roles: Admin | Planner | Operator | QC | Manager
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Whether this account can log in.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────
    public ICollection<ProductionPlan>?   CreatedPlans    { get; set; }
    public ICollection<WorkOrder>?        CreatedWorkOrders { get; set; }
    public ICollection<ProductionLog>?    ProductionLogs  { get; set; }
    public ICollection<QcResult>?         QcResults       { get; set; }
    public ICollection<AuditLog>?         AuditLogs       { get; set; }
}
