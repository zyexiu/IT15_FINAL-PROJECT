using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 7 — Work Order.
/// Full lifecycle: Draft → Released → InProgress → Completed | Cancelled.
/// Created by Planner, executed by Operator.
/// </summary>
public class WorkOrder
{
    [Key]
    public int WorkOrderId { get; set; }

    /// <summary>System-generated number, e.g. WO-2025-0001.</summary>
    [Required, MaxLength(30)]
    public string WoNumber { get; set; } = string.Empty;

    /// <summary>Finished good to produce.</summary>
    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    /// <summary>BOM used to explode materials for this WO.</summary>
    [Required]
    public int BomId { get; set; }

    [ForeignKey(nameof(BomId))]
    public BillOfMaterials? Bom { get; set; }

    /// <summary>Optional link to the plan line that generated this WO.</summary>
    public int? PlanLineId { get; set; }

    [ForeignKey(nameof(PlanLineId))]
    public PlanLine? PlanLine { get; set; }

    [Required, Column(TypeName = "decimal(12,4)")]
    public decimal PlannedQty { get; set; }

    [Column(TypeName = "decimal(12,4)")]
    public decimal ActualQty { get; set; } = 0;

    [Required, MaxLength(15)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>Draft | Released | InProgress | Completed | Cancelled</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Draft";

    [MaxLength(80)]
    public string? ProductionLine { get; set; }

    [Required]
    public DateTime ScheduledStart { get; set; }

    [Required]
    public DateTime ScheduledEnd { get; set; }

    public DateTime? ActualStart { get; set; }
    public DateTime? ActualEnd   { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required, MaxLength(450)]
    public string CreatedByUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────
    public ICollection<WorkOrderMaterial>? Materials      { get; set; }
    public ICollection<ProductionLog>?     ProductionLogs { get; set; }
    public ICollection<QcResult>?          QcResults      { get; set; }
    public WorkOrderCost?                  Cost           { get; set; }
    public ICollection<InventoryLedger>?   InventoryLedgers { get; set; }
}
