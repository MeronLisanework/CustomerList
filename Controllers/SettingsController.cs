using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerList.Controllers;

[Authorize(Roles = "Administrator")]
public class SettingsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}