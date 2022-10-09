using IdentityManager.Models;
using IdentityManager.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmailSender _emailSender;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
    }


    // GET
    [HttpGet]
    public IActionResult Register(string? returnUrl=null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        RegisterVm registerVm = new RegisterVm();
        
        return View(registerVm);
    }
    
    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm model, string? returnUrl=null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        returnUrl ??= Url.Content("~/");
        
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

            return LocalRedirect(returnUrl);
        }
        
        AddErrors(result);
        
        return View(model);
    }

    // GET
    [HttpGet]
    public IActionResult Login(string? returnUrl=null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        return View();
    }
    
    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm model, string? returnUrl=null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        returnUrl ??= Url.Content("~/");
        
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password,
            isPersistent: model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
            return View("Lockout");
        
        ModelState.AddModelError(string.Empty, "Invalid login attempt");

        return View(model);
    }

    private void AddErrors(IdentityResult result)
    {
        foreach(var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }

    public async Task<IActionResult> LogOff()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction(nameof(Index), "Home");
    }
    
    // GET
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }
    
    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVm model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callBackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
        var htmlMessage = "Please reset your password by clicking here <a href=\"" + callBackUrl + "\">link</a>";
        
        await _emailSender.SendEmailAsync(model.Email, "Reset Password", htmlMessage);

        return RedirectToAction("ForgotPasswordConfirmation");
    }
    
    // GET
    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    // GET
    public IActionResult ResetPassword(string? code=null)
    {
        return code == null ? View("Error") : View("ResetPassword");
    }
    
    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordVm model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return RedirectToAction("ResetPasswordConfirmation");
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

        if (result.Succeeded)
        {
            return RedirectToAction("ResetPasswordConfirmation");
        }
        AddErrors(result);
        
        return View();
    }
    
    // GET
    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}