using System.ComponentModel.DataAnnotations;

namespace StudyRoomBooking.ViewModels;

public sealed class RegisterViewModel
{
    [Required, StringLength(80, MinimumLength = 2)]
    [Display(Name = "Your name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 2)]
    [Display(Name = "Student number")]
    public string StudentNumber { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(10)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
