using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 9 — QC Result.
/// QC Inspector records pass/fail per work order with notes and disposition.
/// </summary>
public class QcResult
{
    [Key]
    public int QcResultId { get; set; }

    [Required]
    public int WorkOrderId { get; set; }

    [ForeignKey(nameof(WorkOrderId))]
    public WorkOrder? WorkOrder { get; set; }

    public DateTime InspectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Pass | Fail | ConditionalPass</summary>
    [Required, MaxLength(20)]
    public string Result { get; set; } = string.Empty;

    /// <summary>Appearance | Weight | Moisture | Taste | Packaging | Overall</summary>
    [MaxLength(50)]
    public string? CheckType { get; set; }

    [Column(TypeName = "decimal(12,4)")]
    public decimal? SampleQty { get; set; }

    [Column(TypeName = "decimal(12,4)")]
    public decimal? DefectQty { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>What happens to the batch: Accept | Rework | Reject</summary>
    [MaxLength(20)]
    public string? Disposition { get; set; }

    [Required, MaxLength(450)]
    public string InspectedByUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(InspectedByUserId))]
    public ApplicationUser? InspectedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Multi-tenant: Links this QC result to a specific Admin's workspace.</summary>
    [Required, MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;
}
