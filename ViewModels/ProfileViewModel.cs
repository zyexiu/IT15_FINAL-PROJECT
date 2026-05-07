using System.ComponentModel.DataAnnotations;

namespace SnackFlowMES.ViewModels;

/// <summary>
/// View model for user profile editing
/// </summary>
public class ProfileViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, hyphens, and underscores")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    // Read-only display fields
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CurrentUserName { get; set; } = string.Empty; // For validation
    public string CurrentEmail { get; set; } = string.Empty; // For validation
}
