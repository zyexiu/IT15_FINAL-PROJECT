using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SnackFlowMES.Models;

/// <summary>
/// Extends ASP.NET Core Identity user with SnackFlow-specific fields.
/// Roles: Admin | Planner | Operator | QC | Manager
/// Multi-tenant: Each Admin is a tenant, and all users belong to a tenant.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Whether this account can log in.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tenant ID - For Admins, this is their own ID. For other roles, this is their Admin's ID.
    /// This enables multi-tenancy where each Admin has isolated data.
    /// </summary>
    [MaxLength(450)]
    public string? TenantId { get; set; }

    // ── Navigation ──────────────────────────────────────────
    public ICollection<ProductionPlan>?   CreatedPlans    { get; set; }
    public ICollection<WorkOrder>?        CreatedWorkOrders { get; set; }
    public ICollection<ProductionLog>?    ProductionLogs  { get; set; }
    public ICollection<QcResult>?         QcResults       { get; set; }
    public ICollection<AuditLog>?         AuditLogs       { get; set; }
    public ICollection<MaterialRequest>?  MaterialRequests { get; set; }
    public ICollection<Notification>?     ReceivedNotifications { get; set; }
    public ICollection<Notification>?     CreatedNotifications { get; set; }
    public ICollection<DowntimeReport>?   DowntimeReports { get; set; }
}
