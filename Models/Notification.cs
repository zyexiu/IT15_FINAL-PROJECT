using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Notification system for role-based alerts and communication.
/// Enables upward reporting flow (Planner→Admin, Operator→Manager, QC→Admin/Manager).
/// </summary>
public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    /// <summary>User who should receive this notification (null = role-based)</summary>
    [MaxLength(450)]
    public string? RecipientUserId { get; set; }

    [ForeignKey(nameof(RecipientUserId))]
    public ApplicationUser? RecipientUser { get; set; }

    /// <summary>Role that should receive this notification (null = user-specific)</summary>
    [MaxLength(50)]
    public string? RecipientRole { get; set; }

    /// <summary>
    /// Type of notification:
    /// LowStock | MaterialRequest | ProductionComplete | QCFailed | QCPassed | 
    /// DowntimeReport | MachineIssue | WorkOrderCreated | WorkOrderReleased | 
    /// InventoryAlert | SystemAlert
    /// </summary>
    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>Entity type this notification relates to: WorkOrder | Item | MaterialRequest | QcResult</summary>
    [MaxLength(50)]
    public string? RelatedEntityType { get; set; }

    /// <summary>ID of the related entity</summary>
    public int? RelatedEntityId { get; set; }

    /// <summary>URL to navigate to when notification is clicked</summary>
    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    /// <summary>Priority: Low | Medium | High | Critical</summary>
    [Required, MaxLength(20)]
    public string Priority { get; set; } = "Medium";

    /// <summary>User who created this notification (system-generated if null)</summary>
    [MaxLength(450)]
    public string? CreatedByUserId { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser? CreatedBy { get; set; }

    /// <summary>Multi-tenant: Links this notification to a specific Admin's workspace.</summary>
    [Required, MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;
}
