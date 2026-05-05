using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 2 — Items / Materials Master List.
/// Single table for raw materials, packaging, and finished goods.
/// </summary>
public class Item
{
    [Key]
    public int ItemId { get; set; }

    /// <summary>Unique code, e.g. RM-001, PK-001, FG-001.</summary>
    [Required, MaxLength(30)]
    public string ItemCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>RawMaterial | Packaging | FinishedGood | SemiFinished</summary>
    [Required, MaxLength(20)]
    public string ItemType { get; set; } = string.Empty;

    /// <summary>kg | g | L | pcs | bag | box | pack</summary>
    [Required, MaxLength(15)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Category { get; set; }

    /// <summary>Standard cost per UOM — used in cost calculations.</summary>
    [Column(TypeName = "decimal(12,4)")]
    public decimal UnitCost { get; set; } = 0;

    /// <summary>Minimum stock level that triggers a shortage flag.</summary>
    [Column(TypeName = "decimal(12,4)")]
    public decimal ReorderPoint { get; set; } = 0;

    [MaxLength(300)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ──────────────────────────────────────────
    public ICollection<BomLine>?              BomLines          { get; set; }
    public ICollection<WorkOrderMaterial>?    WorkOrderMaterials { get; set; }
    public ICollection<InventoryLedger>?      InventoryLedgers  { get; set; }
    public ICollection<MrpRequirement>?       MrpRequirements   { get; set; }
    public BillOfMaterials?                   Bom               { get; set; }
    public InventoryBalance?                  InventoryBalance  { get; set; }
}
