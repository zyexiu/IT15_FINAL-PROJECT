using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Material Request system for Planner to request materials from Admin.
/// Enables Planner to detect low stock and submit restocking requests.
/// </summary>
public class MaterialRequest
{
    [Key]
    public int RequestId { get; set; }

    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    [Required, Column(TypeName = "decimal(12,4)")]
    public decimal RequestedQty { get; set; }

    [Required, MaxLength(15)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>
    /// Reason for request:
    /// LowStock | UpcomingProduction | Emergency | Reorder | Other
    /// </summary>
    [Required, MaxLength(50)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Status: Pending | Approved | Rejected | Fulfilled | Cancelled
    /// </summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>Priority: Low | Medium | High | Critical</summary>
    [Required, MaxLength(20)]
    public string Priority { get; set; } = "Medium";

    /// <summary>When the material is needed by</summary>
    public DateTime? RequiredByDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>User who requested the material (typically Planner)</summary>
    [Required, MaxLength(450)]
    public string RequestedByUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(RequestedByUserId))]
    public ApplicationUser? RequestedBy { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Admin who approved/rejected the request</summary>
    [MaxLength(450)]
    public string? ApprovedByUserId { get; set; }

    [ForeignKey(nameof(ApprovedByUserId))]
    public ApplicationUser? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    /// <summary>Admin's response notes</summary>
    [MaxLength(1000)]
    public string? ApprovalNotes { get; set; }

    /// <summary>Date when the request was fulfilled</summary>
    public DateTime? FulfilledAt { get; set; }

    /// <summary>Actual quantity fulfilled (may differ from requested)</summary>
    [Column(TypeName = "decimal(12,4)")]
    public decimal? FulfilledQty { get; set; }

    /// <summary>Multi-tenant: Links this request to a specific Admin's workspace.</summary>
    [Required, MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;
}
