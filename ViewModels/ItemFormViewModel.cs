using System.ComponentModel.DataAnnotations;

namespace SnackFlowMES.ViewModels;

public class ItemFormViewModel
{
    public int ItemId { get; set; }

    [Required]
    [MaxLength(30)]
    [Display(Name = "Item Code")]
    public string ItemCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Display(Name = "Item Name")]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Item Type")]
    public string ItemType { get; set; } = "RawMaterial";

    [Required]
    [MaxLength(15)]
    [Display(Name = "Unit of Measure")]
    public string UnitOfMeasure { get; set; } = "kg";

    [MaxLength(80)]
    public string? Category { get; set; }

    [Range(0, 999999.99)]
    [Display(Name = "Unit Cost")]
    public decimal UnitCost { get; set; } = 0;

    [Range(0, 999999.99)]
    [Display(Name = "Reorder Point")]
    public decimal ReorderPoint { get; set; } = 0;

    [MaxLength(300)]
    public string? Description { get; set; }
}
