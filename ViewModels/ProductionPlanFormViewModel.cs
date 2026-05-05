using System.ComponentModel.DataAnnotations;

namespace SnackFlowMES.ViewModels;

public class ProductionPlanFormViewModel
{
    public int PlanId { get; set; }

    [Required(ErrorMessage = "Plan name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Plan name must be between 3 and 100 characters")]
    [Display(Name = "Plan Name")]
    public string PlanName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Plan start date is required")]
    [Display(Name = "Plan Start Date")]
    public DateTime PlanDateFrom { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Plan end date is required")]
    [Display(Name = "Plan End Date")]
    public DateTime PlanDateTo { get; set; } = DateTime.Now.AddDays(7);

    [Required(ErrorMessage = "Status is required")]
    [StringLength(20)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Draft";

    [StringLength(500)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}
