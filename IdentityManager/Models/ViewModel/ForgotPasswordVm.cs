using System.ComponentModel.DataAnnotations;

namespace IdentityManager.Models.ViewModel;

public class ForgotPasswordVm
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
}