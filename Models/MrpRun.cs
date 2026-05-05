using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 6 — MRP Run header.
/// Triggered by a Planner against a Production Plan to compute material requirements.
/// </summary>
public class MrpRun
{
    [Key]
    public int MrpRunId { get; set; }

    [Required]
    public int PlanId { get; set; }

    [ForeignKey(nameof(PlanId))]
    public ProductionPlan? Plan { get; set; }

    public DateTime RunAt { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(450)]
    public string RunByUserId { get; set; } = string.Empty;

    /// <summary>Completed | Failed</summary>
    [MaxLength(15)]
    public string Status { get; set; } = "Completed";

    [MaxLength(300)]
    public string? Notes { get; set; }

    // ── Navigation ──────────────────────────────────────────
    public ICollection<MrpRequirement>? Requirements { get; set; }
}
