using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 7 — Work Order Material allocation.
/// Exploded from BOM when WO is released. Tracks planned vs. actual consumption.
/// </summary>
public class WorkOrderMaterial
{
    [Key]
    public int WoMaterialId { get; set; }

    [Required]
    public int WorkOrderId { get; set; }

    [ForeignKey(nameof(WorkOrderId))]
    public WorkOrder? WorkOrder { get; set; }

    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    [Required, Column(TypeName = "decimal(12,4)")]
    public decimal PlannedQty { get; set; }

    [Column(TypeName = "decimal(12,4)")]
    public decimal ActualQty { get; set; } = 0;

    [Required, MaxLength(15)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>Unit cost at time of WO release (snapshot from Item.UnitCost).</summary>
    [Column(TypeName = "decimal(12,4)")]
    public decimal UnitCostSnapshot { get; set; } = 0;

    /// <summary>ActualQty × UnitCostSnapshot — computed, not stored.</summary>
    [NotMapped]
    public decimal ActualCost => ActualQty * UnitCostSnapshot;
}
