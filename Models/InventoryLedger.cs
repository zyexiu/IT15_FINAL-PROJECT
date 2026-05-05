using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 4 — Inventory Ledger (every stock movement, ever).
/// Immutable append-only log. Positive qty = stock in, negative = stock out.
/// </summary>
public class InventoryLedger
{
    [Key]
    public int LedgerId { get; set; }

    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    /// <summary>
    /// GoodsReceipt | Adjustment | WoIssue | WoReturn | ProductionOutput | Scrap
    /// </summary>
    [Required, MaxLength(25)]
    public string MovementType { get; set; } = string.Empty;

    /// <summary>Signed quantity: positive = in, negative = out.</summary>
    [Required, Column(TypeName = "decimal(12,4)")]
    public decimal Qty { get; set; }

    [MaxLength(15)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>Running balance after this entry (denormalized for fast reads).</summary>
    [Column(TypeName = "decimal(12,4)")]
    public decimal BalanceAfter { get; set; } = 0;

    /// <summary>Optional link to the work order that caused this movement.</summary>
    public int? WorkOrderId { get; set; }

    [ForeignKey(nameof(WorkOrderId))]
    public WorkOrder? WorkOrder { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }

    [MaxLength(450)]
    public string? PostedByUserId { get; set; }

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
}
