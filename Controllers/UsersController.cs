using CustomerList.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CustomerList.Controllers;

[Authorize(Roles = "Administrator")]
public class UsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var userList = new List<UserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "(no email)",
                Roles = roles.ToList(),
                LockedOut = await _userManager.IsLockedOutAsync(user)
            });
        }

        return View(userList);
    }

    public IActionResult Create()
    {
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);
            TempData["SuccessMessage"] = $"User '{model.Email}' was created successfully with the {model.Role} role.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(model);
    }

    public async Task<IActionResult> EditRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        var model = new EditUserRoleViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = currentRoles.FirstOrDefault() ?? string.Empty
        };

        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(EditUserRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        // Prevent an admin from accidentally locking themselves out of the
        // Administrator role entirely -- if this is the last remaining
        // Administrator account, block the change.
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains("Administrator") && model.Role != "Administrator")
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Administrator");
            if (adminUsers.Count <= 1)
            {
                ModelState.AddModelError(string.Empty, "Cannot change this user's role -- they are the only remaining Administrator.");
                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }
        }

        // Remove all existing roles, then assign the newly selected one --
        // this app's design is one role per user, not multiple at once.
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, model.Role);

        TempData["SuccessMessage"] = $"{user.Email}'s role was updated to {model.Role}.";
        return RedirectToAction(nameof(Index));
    }

}