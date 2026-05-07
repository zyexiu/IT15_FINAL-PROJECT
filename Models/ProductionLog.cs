using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnackFlowMES.Models;

/// <summary>
/// Module 8 — Production Log entry.
/// Operator records produced qty, scrap qty, shift, and labor/machine time per entry.
/// Multiple logs can exist per work order (one per shift or per day).
/// </summary>
public class ProductionLog
{
    [Key]
    public int LogId { get; set; }

    [Required]
    public int WorkOrderId { get; set; }

    [ForeignKey(nameof(WorkOrderId))]
    public WorkOrder? WorkOrder { get; set; }

    [Required]
    public DateTime LogDate { get; set; }

    /// <summary>Morning | Afternoon | Night</summary>
    [MaxLength(15)]
    public string? Shift { get; set; }

    [Required, Column(TypeName = "decimal(12,4)")]
    public decimal ProducedQty { get; set; }

    [Column(TypeName = "decimal(12,4)")]
    public decimal ScrapQty { get; set; } = 0;

    [MaxLength(15)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>Actual labor hours worked this entry.</summary>
    [Column(TypeName = "decimal(8,2)")]
    public decimal LaborHours { get; set; } = 0;

    /// <summary>Actual machine hours used this entry.</summary>
    [Column(TypeName = "decimal(8,2)")]
    public decimal MachineHours { get; set; } = 0;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required, MaxLength(450)]
    public string RecordedByUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(RecordedByUserId))]
    public ApplicationUser? RecordedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Multi-tenant: Links this production log to a specific Admin's workspace.</summary>
    [Required, MaxLength(450)]
    public string TenantId { get; set; } = string.Empty;
}
