using IdentityManager.Models;
using IdentityManager.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }


    // GET
    public IActionResult Register()
    {
        RegisterVm registerVm = new RegisterVm();
        
        return View(registerVm);
    }
    
    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser()
        {
            UserName = model.Email,
            Email = model.Email,
            Name = model.Name
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent:false);

            return RedirectToAction(nameof(Index), "Home");
        }
        
        AddErrors(result);
        
        return View(model);
    }


    private void AddErrors(IdentityResult result)
    {
        foreach(var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }
}