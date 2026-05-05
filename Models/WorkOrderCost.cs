using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 10 — Work Order Cost Summary.
/// One row per completed work order. Computed from material usage, labor, and machine time.
/// </summary>
public class WorkOrderCost
{
    [Key]
    public int CostId { get; set; }

    /// <summary>One-to-one with WorkOrder.</summary>
    [Required]
    public int WorkOrderId { get; set; }

    [ForeignKey(nameof(WorkOrderId))]
    public WorkOrder? WorkOrder { get; set; }

    /// <summary>Sum of (ActualQty × UnitCostSnapshot) across all WO materials.</summary>
    [Column(TypeName = "decimal(14,2)")]
    public decimal MaterialCost { get; set; } = 0;

    /// <summary>Sum of (LaborHours × LaborRatePerHour) across all production logs.</summary>
    [Column(TypeName = "decimal(14,2)")]
    public decimal LaborCost { get; set; } = 0;

    /// <summary>Sum of (MachineHours × MachineRatePerHour) across all production logs.</summary>
    [Column(TypeName = "decimal(14,2)")]
    public decimal MachineCost { get; set; } = 0;

    [Column(TypeName = "decimal(14,2)")]
    public decimal OtherCost { get; set; } = 0;

    /// <summary>MaterialCost + LaborCost + MachineCost + OtherCost — computed, not stored.</summary>
    [NotMapped]
    public decimal TotalCost => MaterialCost + LaborCost + MachineCost + OtherCost;

    /// <summary>TotalCost ÷ ActualQty produced — stored for reporting.</summary>
    [Column(TypeName = "decimal(14,4)")]
    public decimal CostPerUnit { get; set; } = 0;

    /// <summary>Labor rate used (PHP/hr) at time of computation.</summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal LaborRatePerHour { get; set; } = 0;

    /// <summary>Machine rate used (PHP/hr) at time of computation.</summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal MachineRatePerHour { get; set; } = 0;

    [MaxLength(300)]
    public string? Notes { get; set; }

    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? ComputedByUserId { get; set; }
}
