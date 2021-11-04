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

        

        private async Task<double[]> Counter(IQueryable<BonRequest> requests)
        {
            double[] price = { 0, 0, 0}; 
            var req = requests.Where(r => r.Status == "Approved").ToList();
            foreach(var r in req)
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
            for(int i=0; i<price.Length; i++)
            {
                price[i] /= 100;
            }
            return price;
        }
        private async Task<RequestsViewModel> Filter(string param)
        {
            var requests = from br in _context.BonRequests select br;
            List<BonRequest> req = requests.ToList();
            if ((param != "All") && (param != null))
            {
                req = requests.Where(r => r.Status == param).ToList();
            }
            List<User> users = new();
            List<Bonus> bonuses = new();
            foreach (BonRequest request in req)
            {
                users.Add(await _userManager.FindByIdAsync(request.UserId));
                bonuses.Add(await _context.Bonuses.FindAsync(request.BonusId));
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
                Price = await Counter(requests)
            };
            return model;
        }


        // GET: BonRequests
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await Filter("Pending");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int Id, string command)
        {
            BonRequest request = await _context.BonRequests.FindAsync(Id);
            request.Status = command;
            _context.Update(request);
            await _context.SaveChangesAsync();

            var requests = from br in _context.BonRequests select br;
            var req = requests.Where(br => (br.BonusId == request.BonusId) && (br.UserId == request.UserId) && (br.Status == "Pending")).ToList();
            foreach(var r in req){
                r.Status = "Rejected";
                _context.Update(r);
            }
            await _context.SaveChangesAsync();
            var model = await Filter("Pending");
            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> Statistics(string State)
        {
            var model = await Filter(State);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Statistics()
        {
            
            return View();
        }
    }
}
