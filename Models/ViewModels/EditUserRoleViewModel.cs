using System.ComponentModel.DataAnnotations;

namespace CustomerList.Models.ViewModels;

public class EditUserRoleViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a role.")]
    public string Role { get; set; } = string.Empty;
}