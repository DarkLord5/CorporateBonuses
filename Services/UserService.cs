using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CorporateBonuses.Models;
using CorporateBonuses.ViewModels;
using System.Collections.Generic;

namespace CorporateBonuses.Services
{
    public class UserService
    {
        readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<UserViewModel>> ShowUserListAsync(User CurrentUser)
        {
            var users = _userManager.Users.Where(u => u.Id != CurrentUser.Id).ToList();
            List<UserViewModel> models = new();
            foreach (User user in users)
            {
                 models.Add(await ShowUserAsync(user));
            }
            return models;
        }

        public async Task<UserViewModel> ShowUserAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            UserViewModel model = new()
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FirstName + user.Surname,
                Rang = user.Rang,
                Role = roles[0]
            };
            return model;
        }

        public async Task<IdentityResult> EditUserAsync(User user, UserViewModel model)
        {
            user.Rang = model.Rang;
            var roles = await _userManager.GetRolesAsync(user);
            if ((model.Role == "admin")||(model.Role=="user")) {
                await _userManager.RemoveFromRoleAsync(user, roles[0]);
                await _userManager.AddToRoleAsync(user, model.Role);
            }
            return await _userManager.UpdateAsync(user);
        }

        
    }
}
