using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CorporateBonuses.Models;
using CorporateBonuses.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using CorporateBonuses.Services;

namespace CorporateBonuses.Controllers
{
    public class BonRequestsController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;

        public BonRequestsController(ApplicationContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        //вспомогательные методы

        public double[] Counter(IQueryable<BonRequest> requests)
        {
            double[] price = { 0, 0, 0 };
            var req = requests.Where(r => r.Status == "Approved").ToList();
            foreach (var r in req)
            {
                price[0] += r.Price;
            }
            req = requests.Where(r => r.Status == "Pending").ToList();
            foreach (var r in req)
            {
                price[1] += r.Price;
            }
            req = requests.Where(r => r.Status == "Rejected").ToList();
            foreach (var r in req)
            {
                price[2] += r.Price;
            }
            for (int i = 0; i < price.Length; i++)
            {
                price[i] /= 100;
            }
            return price;
        }


        private async Task<RequestsViewModel> Filter(string param, IQueryable<BonRequest> requests)
        {
            List<BonRequest> req = requests.ToList();
            if ((param != "All") && (param != null))
            {
                req = requests.Where(r => r.Status == param).ToList();
            }
            List<User> users = new();
            List<Bonus> bonuses = new();
            foreach (BonRequest request in req)
            {
                User user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    user = new User
                    {
                        FirstName = "Former",
                        Surname = "Employee"
                    };
                }
                users.Add(user);
                Bonus bonus = await _context.Bonuses.FindAsync(request.BonusId);
                if (bonus == null)
                {
                    bonus = new Bonus { Name="Deleted Bonus" };
                }
                bonuses.Add(bonus);
            }
            List<string> states = new()
            {
                "Pending",
                "Rejected",
                "Approved"
            };
            RequestsViewModel model = new()
            {
                Requests = req,
                Users = users,
                Bonuses = bonuses,
                States = new SelectList(states),
                Price = Counter(requests)
            };
            return model;
        }


        


        // Списки для админа
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var requests = from br in _context.BonRequests select br;
            var model = await Filter("Pending", requests);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(int Id, string command)
        {
            BonRequest request = await _context.BonRequests.FindAsync(Id);
            request.Status = command;
            request.ApproveDate = DateTime.Today;
            _context.Update(request);
            await _context.SaveChangesAsync();
            Bonus bonus = await _context.Bonuses.FindAsync(request.BonusId);
            var persBonuses = _context.PersonalBonuses.Where(pb => request.BonusId == pb.BonusId).Where(pb => request.UserId == pb.UserId).Where(pb => pb.EnableDate > DateTime.Today).ToList();
            if (command == "Approved") {
                if (persBonuses.Count > 0) { 
                var persBon = persBonuses[0];
                persBon.EnableDate = DataCounterService.EnableDate(bonus.DaysToReset);
                _context.Update(persBon);
                }
                else
                {
                    PersonalBonus personalBonus = new()
                    {
                        BonusId = request.BonusId,
                        UserId = request.UserId,
                        EnableDate = DataCounterService.EnableDate(bonus.DaysToReset)
                    };
                    _context.Add(personalBonus);
                }
            }
            else if((command == "Rejected")&&(persBonuses.Count!=0))
            {
                foreach(var persBon in persBonuses) { 
                persBon.EnableDate = DateTime.Today;
                _context.Update(persBon);
                }
            }
            var requests = from br in _context.BonRequests select br;
            var req = requests.Where(br => (br.BonusId == request.BonusId) && (br.UserId == request.UserId) && (br.Status == "Pending")).ToList();
            foreach(var r in req){
                r.Status = "Rejected";
                r.ApproveDate = DateTime.Today;
                _context.Update(r);
            }
            await _context.SaveChangesAsync();
            var model = await Filter("Pending",requests);
            return View(model);
        }


        //Статистика для админа
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Statistics(string State)
        {
            var requests = from br in _context.BonRequests select br;
            var model = await Filter(State, requests);
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Statistics(RequestsViewModel model)
        {
            var requests = from br in _context.BonRequests select br;
            requests = requests.Where(r => (r.ApproveDate >= model.Start) && (r.ApproveDate <= model.End));
            var model2 = await Filter("All", requests);
            return View(model2);
        }


        //Пользовательские методы
        [HttpGet]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> MyRequests(string State)
        {
            string u = User.Identity.Name;
            User user = await _userManager.FindByNameAsync(u);
            var requests = from br in _context.BonRequests select br;
            requests = requests.Where(r => r.UserId == user.Id);
            var model = await Filter(State, requests);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> MyRequests(RequestsViewModel model)
        {
            string u = User.Identity.Name;
            User user = await _userManager.FindByNameAsync(u);
            var requests = from br in _context.BonRequests select br;
            requests = requests.Where(r => r.UserId == user.Id);
            requests = requests.Where(r => (r.ApproveDate >= model.Start) && (r.ApproveDate <= model.End));
            var model2 = await Filter("All", requests);
            return View(model2);
        }
    }
}
