using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 6 — MRP Requirement line.
/// One row per material computed by an MRP run.
/// Shows gross requirement, current stock, and net requirement (shortage).
/// </summary>
public class MrpRequirement
{
    [Key]
    public int MrpReqId { get; set; }

    [Required]
    public int MrpRunId { get; set; }

    [ForeignKey(nameof(MrpRunId))]
    public MrpRun? MrpRun { get; set; }

    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    [Column(TypeName = "decimal(12,4)")]
    public decimal GrossRequirement { get; set; }

    [Column(TypeName = "decimal(12,4)")]
    public decimal StockOnHand { get; set; }

    /// <summary>GrossRequirement - StockOnHand. Positive = shortage.</summary>
    [Column(TypeName = "decimal(12,4)")]
    public decimal NetRequirement { get; set; }

    /// <summary>True when NetRequirement > 0.</summary>
    public bool IsShortage { get; set; } = false;
}
