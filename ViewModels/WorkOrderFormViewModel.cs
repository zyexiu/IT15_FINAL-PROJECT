using System.ComponentModel.DataAnnotations;

namespace SnackFlowMES.ViewModels;

public class WorkOrderFormViewModel
{
    public int WorkOrderId { get; set; }

    [Required(ErrorMessage = "Item is required")]
    [Display(Name = "Finished Good")]
    public int ItemId { get; set; }

    [Required(ErrorMessage = "BOM is required")]
    [Display(Name = "Bill of Materials")]
    public int BomId { get; set; }

    [Required]
    [Range(0.01, 999999.99, ErrorMessage = "Planned quantity must be greater than 0")]
    [Display(Name = "Planned Quantity")]
    public decimal PlannedQty { get; set; }

    [Required]
    [MaxLength(15)]
    [Display(Name = "Unit of Measure")]
    public string UnitOfMeasure { get; set; } = "kg";

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    [MaxLength(80)]
    [Display(Name = "Production Line")]
    public string? ProductionLine { get; set; }

    [Required]
    [Display(Name = "Scheduled Start")]
    public DateTime ScheduledStart { get; set; } = DateTime.Now.AddDays(1);

    [Required]
    [Display(Name = "Scheduled End")]
    public DateTime ScheduledEnd { get; set; } = DateTime.Now.AddDays(2);

    [MaxLength(500)]
    public string? Notes { get; set; }
}
