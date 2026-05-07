using System.ComponentModel.DataAnnotations;

namespace SnackFlowMES.ViewModels;

public class PlanLineFormViewModel
{
    public int PlanLineId { get; set; }

    [Required]
    public int PlanId { get; set; }

    [Required(ErrorMessage = "Please select a finished good")]
    [Display(Name = "Finished Good")]
    public int ItemId { get; set; }

    [Required(ErrorMessage = "Planned quantity is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Quantity must be greater than 0")]
    [Display(Name = "Planned Quantity")]
    public decimal PlannedQty { get; set; }

    [Required(ErrorMessage = "Unit of measure is required")]
    [StringLength(15)]
    [Display(Name = "Unit of Measure")]
    public string UnitOfMeasure { get; set; } = string.Empty;

    [Required(ErrorMessage = "Scheduled date is required")]
    [Display(Name = "Scheduled Date")]
    public DateTime ScheduledDate { get; set; } = DateTime.Now;

    [StringLength(80)]
    [Display(Name = "Production Line")]
    public string? ProductionLine { get; set; }

    [StringLength(300)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}
