using System.ComponentModel.DataAnnotations;

namespace SnackFlowMES.Models;

public class DowntimeReport
{
    [Key]
    public int DowntimeId { get; set; }

    [Required]
    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    [Required]
    [StringLength(100)]
    public string ProductionLine { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int DurationMinutes { get; set; }

    [Required]
    [StringLength(50)]
    public string Reason { get; set; } = string.Empty;
    // "Machine Breakdown" | "Material Shortage" | "Power Outage" | "Maintenance" | "Changeover" | "Other"

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(450)]
    public string ReportedByUserId { get; set; } = string.Empty;
    public ApplicationUser? ReportedBy { get; set; }

    [Required]
    public DateTime ReportedAt { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Open";
    // "Open" | "Resolved" | "Escalated"

    [StringLength(500)]
    public string? Resolution { get; set; }

    [StringLength(450)]
    public string? ResolvedByUserId { get; set; }
    public ApplicationUser? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    // Multi-tenancy
    [Required]
    [StringLength(100)]
    public string OrganizationId { get; set; } = string.Empty;
}

