using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityManager.Models.ViewModel;

public class RegisterVm
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "The {0} must be {1} character long.")]
    public string Password { get; set; }
    
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and Confirm Password Must be matched")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; }

    public string? Name { get; set; }

    public IEnumerable<SelectListItem>? RoleList { get; set; }
    public string? RoleSelected { get; set; }
    
}