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

    [Required, MaxLength(450)]
    public string CreatedByUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────
    public ICollection<PlanLine>?  PlanLines { get; set; }
    public ICollection<MrpRun>?    MrpRuns   { get; set; }
}
