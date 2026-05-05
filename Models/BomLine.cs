using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 3 — BOM Component Line.
/// Each row = one ingredient or packaging material required per BOM batch.
/// </summary>
public class BomLine
{
    [Key]
    public int BomLineId { get; set; }

    [Required]
    public int BomId { get; set; }

    [ForeignKey(nameof(BomId))]
    public BillOfMaterials? Bom { get; set; }

    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    /// <summary>Quantity of this component needed per BOM batch.</summary>
    [Required, Column(TypeName = "decimal(12,4)")]
    public decimal QtyPerBatch { get; set; }

    [Required, MaxLength(15)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>Expected waste/scrap percentage (0–100).</summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal ScrapPct { get; set; } = 0;

    [MaxLength(200)]
    public string? Notes { get; set; }
}
