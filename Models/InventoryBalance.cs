using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 4 — Inventory Balance (current on-hand snapshot per item).
/// One row per item. Updated whenever an InventoryLedger entry is posted.
/// </summary>
public class InventoryBalance
{
    [Key]
    public int BalanceId { get; set; }

    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    [Column(TypeName = "decimal(12,4)")]
    public decimal QtyOnHand { get; set; } = 0;

    /// <summary>Quantity reserved/allocated to open work orders.</summary>
    [Column(TypeName = "decimal(12,4)")]
    public decimal QtyReserved { get; set; } = 0;

    /// <summary>QtyOnHand minus QtyReserved — computed, not stored.</summary>
    [NotMapped]
    public decimal QtyAvailable => QtyOnHand - QtyReserved;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
