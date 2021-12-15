using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CorporateBonuses.Models;
using CorporateBonuses.ViewModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using CorporateBonuses.Services;

namespace CustomIdentityApp.Controllers
{
    public class UsersController : Controller
    {
        readonly UserManager<User> _userManager;
        readonly UserService _userService;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
            _userService = new(userManager);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var CurrentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            return View(await _userService.ShowUserListAsync(CurrentUser));
        }


        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(await _userService.ShowUserAsync(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    IdentityResult result = await _userService.EditUserAsync(user, model);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return View(model);
        }

    }
}