using System.ComponentModel.DataAnnotations;

namespace SnackFlowMES.ViewModels;

public class BomFormViewModel
{
    public int BomId { get; set; }

    [Required(ErrorMessage = "Item is required")]
    [Display(Name = "Finished Good")]
    public int ItemId { get; set; }

    [Required(ErrorMessage = "Version is required")]
    [StringLength(10, MinimumLength = 1)]
    [Display(Name = "Version")]
    public string Version { get; set; } = "v1";

    [Required(ErrorMessage = "Batch output quantity is required")]
    [Range(0.01, 999999.99)]
    [Display(Name = "Batch Output Quantity")]
    public decimal BatchOutputQty { get; set; }

    [Required(ErrorMessage = "Unit of measure is required")]
    [StringLength(15)]
    [Display(Name = "Unit of Measure")]
    public string BatchOutputUom { get; set; } = string.Empty;

    [Range(0, 9999.99)]
    [Display(Name = "Est. Machine Hours")]
    public decimal EstMachineHours { get; set; } = 0;

    [Range(0, 9999.99)]
    [Display(Name = "Est. Labor Hours")]
    public decimal EstLaborHours { get; set; } = 0;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}

public class BomLineFormViewModel
{
    public int BomLineId { get; set; }
    public int BomId { get; set; }

    [Required(ErrorMessage = "Material is required")]
    [Display(Name = "Raw Material")]
    public int ItemId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(0.01, 999999.99)]
    [Display(Name = "Quantity")]
    public decimal Quantity { get; set; }

    [Required(ErrorMessage = "Unit of measure is required")]
    [StringLength(15)]
    [Display(Name = "Unit of Measure")]
    public string UnitOfMeasure { get; set; } = string.Empty;

    [StringLength(300)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}
