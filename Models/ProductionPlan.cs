using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 5 — Production Plan header.
/// A plan covers a date range and contains one or more plan lines.
/// Created by Planner/Scheduler.
/// </summary>
public class ProductionPlan
{
    [Key]
    public int PlanId { get; set; }

    [Required, MaxLength(100)]
    public string PlanName { get; set; } = string.Empty;

    [Required]
    public DateTime PlanDateFrom { get; set; }

    [Required]
    public DateTime PlanDateTo { get; set; }

    /// <summary>Draft | Approved | InProgress | Completed | Cancelled</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Draft";

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(450)]
    public string? CreatedByUserId { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Multi-tenant: Links this plan to a specific Admin's workspace.</summary>
    [Required, MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    // ── Navigation ──────────────────────────────────────────
    public ICollection<PlanLine>?  PlanLines { get; set; }
}
