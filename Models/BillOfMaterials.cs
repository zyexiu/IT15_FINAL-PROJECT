using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 3 — BOM Header.
/// One BOM per finished/semi-finished item per version. Multiple versions allowed.
/// </summary>
public class BillOfMaterials
{
    [Key]
    public int BomId { get; set; }

    /// <summary>The finished good this BOM produces. One-to-one with Item.</summary>
    [Required]
    public int ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }

    /// <summary>Version label, e.g. v1, v2.</summary>
    [Required, MaxLength(10)]
    public string Version { get; set; } = "v1";

    /// <summary>Standard output quantity per one production run.</summary>
    [Required, Column(TypeName = "decimal(12,4)")]
    public decimal BatchOutputQty { get; set; }

    [MaxLength(15)]
    public string BatchOutputUom { get; set; } = string.Empty;

    /// <summary>Estimated machine hours needed per batch.</summary>
    [Column(TypeName = "decimal(8,2)")]
    public decimal EstMachineHours { get; set; } = 0;

    /// <summary>Estimated labor hours needed per batch.</summary>
    [Column(TypeName = "decimal(8,2)")]
    public decimal EstLaborHours { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Multi-tenant: Links this BOM to a specific Admin's workspace.</summary>
    [Required, MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;

    // ── Navigation ──────────────────────────────────────────
    public ICollection<BomLine>?  BomLines   { get; set; }
    public ICollection<WorkOrder>? WorkOrders { get; set; }
}
