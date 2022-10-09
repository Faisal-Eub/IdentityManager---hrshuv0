using System.ComponentModel.DataAnnotations;

namespace IdentityManager.Models.ViewModel;

public class ExternalLoginConfirmationVm
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Name { get; set; }
}