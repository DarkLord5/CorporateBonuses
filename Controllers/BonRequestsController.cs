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
        private double[] Counter(IQueryable<BonRequest> requests)
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


        private DateTime EnableDate(int expectation)
        {
            DateTime newDate = DateTime.Today;
            if(expectation%30==0)
            {
                int month = expectation / 30;
                newDate = newDate.AddMonths(month);
                
                return new DateTime(newDate.Year, newDate.Month, 1);
            }
            else if(expectation == 365)
            {
                newDate = newDate.AddYears(1);
                return new DateTime(newDate.Year, 1, 1);
            }
            else
            {
                return newDate.AddDays(expectation);
            }
        }


        // Списки для админа
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requests = from br in _context.BonRequests select br;
            var model = await Filter("Pending", requests);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int Id, string command)
        {
            BonRequest request = await _context.BonRequests.FindAsync(Id);
            request.Status = command;
            request.ApproveDate = DateTime.Today;
            _context.Update(request);
            await _context.SaveChangesAsync();
            if (command == "Approved") { 
            Bonus bonus = await _context.Bonuses.FindAsync(request.BonusId);
            PersonalBonus persBon = new()
            {
                UserId = request.UserId,
                BonusId = request.BonusId,
                EnableDate = EnableDate(bonus.DaysToReset)
            };
            _context.Add(persBon);
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
        public async Task<IActionResult> Statistics(string State)
        {
            var requests = from br in _context.BonRequests select br;
            var model = await Filter(State, requests);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Statistics(RequestsViewModel model)
        {
            var requests = from br in _context.BonRequests select br;
            requests = requests.Where(r => (r.ApproveDate >= model.Start) && (r.ApproveDate <= model.End));
            var model2 = await Filter("All", requests);
            return View(model2);
        }


        //Пользовательские методы
        [HttpGet]
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
