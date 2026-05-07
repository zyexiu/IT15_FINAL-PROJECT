using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Security — Audit / Activity Log.
/// Immutable record of every significant action in the system.
/// Covers: work order status changes, QC results, inventory adjustments, logins.
/// </summary>
public class AuditLog
{
    [Key]
    public int AuditLogId { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [MaxLength(100)]
    public string? UserName { get; set; }

    /// <summary>
    /// WorkOrder | QcResult | Inventory | ProductionLog | UserManagement | Login
    /// </summary>
    [Required, MaxLength(40)]
    public string Module { get; set; } = string.Empty;

    /// <summary>Create | Update | Delete | StatusChange | Login | Logout | MrpRun</summary>
    [Required, MaxLength(30)]
    public string Action { get; set; } = string.Empty;

    /// <summary>PK of the affected record (e.g. WorkOrderId = 42).</summary>
    public int? EntityId { get; set; }

    /// <summary>Human-readable description of what changed.</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>JSON of old field values (for updates).</summary>
    [Column(TypeName = "text")]
    public string? OldValues { get; set; }

    /// <summary>JSON of new field values.</summary>
    [Column(TypeName = "text")]
    public string? NewValues { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Multi-tenant: Links this audit log to a specific Admin's workspace.</summary>
    [MaxLength(450)]
    public string? TenantId { get; set; }
}
