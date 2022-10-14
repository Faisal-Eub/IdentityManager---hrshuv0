using System.Security.Claims;
using IdentityManager.Models;
using IdentityManager.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdentityManager.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmailSender _emailSender;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
        IEmailSender emailSender, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _roleManager = roleManager;
    }


    // GET
    [HttpGet]
    public async Task<IActionResult> Register(string? returnUrl = null)
    {
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        List<SelectListItem> listItems = new List<SelectListItem>()
        {
            new ()
            {
                Value = "Admin",
                Text = "Admin"
            },
            new()
            {
                Value = "User",
                Text = "User"
            }
        };
        

        ViewData["ReturnUrl"] = returnUrl;

        RegisterVm registerVm = new RegisterVm()
        {
            RoleList = listItems
        };

        return View(registerVm);
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        returnUrl ??= Url.Content("~/");
        
        List<SelectListItem> listItems = new List<SelectListItem>()
        {
            new ()
            {
                Value = "Admin",
                Text = "Admin"
            },
            new()
            {
                Value = "User",
                Text = "User"
            }
        };

        if (!ModelState.IsValid)
        {
            model.RoleList = listItems;
            return View(model);
        }

        var user = new ApplicationUser()
        {
            UserName = model.Email,
            Email = model.Email,
            Name = model.Name
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            if (model.RoleSelected is { Length: > 0 } and "Admin")
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callBackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code },
                protocol: HttpContext.Request.Scheme);
            var htmlMessage = "Please confirm your account by clicking here <a href=\"" + callBackUrl + "\">link</a>";

            await _emailSender.SendEmailAsync(model.Email, "Confirm Email", htmlMessage);
            await _signInManager.SignInAsync(user, isPersistent: false);

            return LocalRedirect(returnUrl);
        }

        AddErrors(result);

        return View(model);
    }

    // GET
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        return View();
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm model, string? returnUrl = null)
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
        foreach (var error in result.Errors)
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
        var callBackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code },
            protocol: HttpContext.Request.Scheme);
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
    public IActionResult ResetPassword(string? code = null)
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

    public async Task<IActionResult> ConfirmEmail(string? userid, string? code)
    {
        if (userid is null || code is null)
        {
            return View("Error");
        }

        var user = await _userManager.FindByIdAsync(userId: userid);
        if (user is null)
            return View("Error");

        var result = await _userManager.ConfirmEmailAsync(user, code);

        return result.Succeeded ? View("ConfirmEmail") : View("Error");
        return View(result.Succeeded ? "ConfirmEmail" : "Error");
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl=null)
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string? remoteError=null)
    {
        returnUrl ??= Url.Content("~/");
        
        if (remoteError is not null)
        {
            ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
            return View(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return RedirectToAction(nameof(Login));
        }
        
        // Sign in the user with this external login provider, if the user already has a login
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent:false);
        if (result.Succeeded)
        {
            //update any authentication token
            await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
            return LocalRedirect(returnUrl);
        }
        else
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);
            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationVm{Email = email, Name = name});
        }
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationVm model, string? returnUrl=null)
    {
        returnUrl ??= Url.Content("~/");
        
        if (ModelState.IsValid)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info is null)
            {
                return View("Error");
            }

            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name
            };

            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                result = await _userManager.AddLoginAsync(user, info);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                    return LocalRedirect(returnUrl);
                }
            }
            
            AddErrors(result);
        }
        
        ViewData["ReturnUrl"] = returnUrl;
        
        return View(model);
    }
}