using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace StudyRoomBooking.Models;

public sealed class ApplicationUser : IdentityUser
{
    [Required, StringLength(80)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string StudentNumber { get; set; } = string.Empty;
}
