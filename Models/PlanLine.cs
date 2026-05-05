using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 5 — Production Plan Line.
/// One line = one finished good to produce on a specific date.
/// </summary>
public class PlanLine
{
    [Key]
    public int PlanLineId { get; set; }

    [Required]
    public int PlanId { get; set; }

    [ForeignKey(nameof(PlanId))]
    public ProductionPlan? Plan { get; set; }

    /// <summary>Finished good to produce.</summary>
    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    [Required, Column(TypeName = "decimal(12,4)")]
    public decimal PlannedQty { get; set; }

    [Required, MaxLength(15)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledDate { get; set; }

    /// <summary>Production line / machine assignment, e.g. Line 1, Oven A.</summary>
    [MaxLength(80)]
    public string? ProductionLine { get; set; }

    /// <summary>Pending | WorkOrderCreated | Completed</summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(300)]
    public string? Notes { get; set; }

    // ── Navigation ──────────────────────────────────────────
    public ICollection<WorkOrder>? WorkOrders { get; set; }
}
