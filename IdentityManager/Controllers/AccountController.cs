using IdentityManager.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace IdentityManager.Controllers;

public class AccountController : Controller
{
    // GET
    public IActionResult Register()
    {
        RegisterVm registerVm = new RegisterVm();
        
        return View(registerVm);
    }
}