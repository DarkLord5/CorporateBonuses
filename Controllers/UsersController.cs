using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CorporateBonuses.Models;
using CorporateBonuses.ViewModels;
using System.Collections.Generic;

namespace CustomIdentityApp.Controllers
{
    public class UsersController : Controller
    {
        readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            List<UserViewModel> model = new();
            
            foreach(User user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                UserViewModel m = new()
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FirstName + user.Surname,
                    Rang = user.Rang,
                    Role= roles[0]
                };
                model.Add(m);
            }
            return View(model);
        }

        
        public async Task<IActionResult> Edit(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            UserViewModel model = new()
            { 
                Id = user.Id,
                Email = user.Email,
                FullName = user.FirstName + user.Surname,
                Rang = user.Rang,
                Role = roles[0]
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    user.Rang = model.Rang;
                    await _userManager.RemoveFromRoleAsync(user, roles[0]);
                    await _userManager.AddToRoleAsync(user, model.Role);
                    var result = await _userManager.UpdateAsync(user);
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