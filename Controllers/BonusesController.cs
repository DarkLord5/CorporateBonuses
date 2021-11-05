using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CorporateBonuses.Models;
using Microsoft.AspNetCore.Identity;
using CorporateBonuses.ViewModels;
using Microsoft.AspNetCore.Authorization;
using CorporateBonuses.Services;

namespace CorporateBonuses.Controllers
{
    public class BonusesController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;
        private readonly DataCounterService _service = new();

        public BonusesController(ApplicationContext context, UserManager<User> userManager )
        {
            _context = context;
            _userManager = userManager;
        }

        private List<BonusViewModel> ToViewModel(List<Bonus> bonuses)
        {
            List<BonusViewModel> bonusViews = new();
            foreach (var bonus in bonuses)
            {
                BonusViewModel bView = new()
                {
                    Id = bonus.Id,
                    Name = bonus.Name,
                    Price = bonus.Price,
                    Enabled = bonus.Enabled,
                    DaysToReset = bonus.DaysToReset,
                    Description = bonus.Description,
                    Rang = bonus.Rang
                };
                bonusViews.Add(bView);
            }
            return bonusViews;
        }

        private List<BonusViewModel> Filtration(User user)
        {
            var persbonuses = from pb in _context.PersonalBonuses select pb;
            var persbon = persbonuses.Where(pb => pb.UserId == user.Id).Where(pb => pb.EnableDate > DateTime.Today);
            var bonuses = _context.Bonuses.Where(b => !persbon.Select(p => p.BonusId).Contains(b.Id)).Where(b => b.Enabled).Where(b => b.Rang <= user.Rang).ToList();
            
            return ToViewModel(bonuses);
        }

        [Authorize(Roles = "user")]
        public async Task<IActionResult> UserList()
        {
            string u = User.Identity.Name;
            User user = await _userManager.FindByNameAsync(u);
            return View(Filtration(user));
        }


        [Authorize(Roles = "user")]
        [HttpPost]
        public async Task<IActionResult> UserList(int Id)
        {
            string u = User.Identity.Name;
            User user = await _userManager.FindByNameAsync(u);
            Bonus bon = await _context.Bonuses.FindAsync(Id);
            BonRequest req = new() { UserId = user.Id, BonusId = Id,  ApproveDate = DateTime.Today, Price = bon.Price };
            if (bon.Rang>1) {

                req.Status = "Pending";
            }
            else
            {
                req.Status = "Approved";
                PersonalBonus persBon = new()
                {
                    UserId = req.UserId,
                    BonusId = req.BonusId,
                    EnableDate = _service.EnableDate(bon.DaysToReset)
                };
                _context.Add(persBon);
            }
            _context.Add(req);
            await _context.SaveChangesAsync();
            return View(Filtration(user));
        }
        // GET: Bonuses
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var bonuses = await _context.Bonuses.ToListAsync();
            return View(ToViewModel(bonuses));
        }

        // GET: Bonuses/Details/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonus = await _context.Bonuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bonus == null)
            {
                return NotFound();
            }
            BonusViewModel bonusView = new()
            {
                Id = bonus.Id,
                Name = bonus.Name,
                Price = bonus.Price,
                Enabled = bonus.Enabled,
                DaysToReset = bonus.DaysToReset,
                Description = bonus.Description,
                Rang = bonus.Rang
            };
            return View(bonusView);
        }

        // GET: Bonuses/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Bonuses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(BonusViewModel bonus)
        {
            Bonus newBonus = new()
            {
                Name = bonus.Name,
                Price = bonus.Price,
                Enabled = bonus.Enabled,
                DaysToReset = bonus.DaysToReset,
                Description = bonus.Description,
                Rang = bonus.Rang
            };
            if (ModelState.IsValid)
            {
                _context.Add(newBonus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bonus);
        }

        // GET: Bonuses/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonus = await _context.Bonuses.FindAsync(id);
            if (bonus == null)
            {
                return NotFound();
            }
            BonusViewModel bonusView = new()
            {
                Id = bonus.Id,
                Name = bonus.Name,
                Price = bonus.Price,
                Enabled = bonus.Enabled,
                DaysToReset = bonus.DaysToReset,
                Description = bonus.Description,
                Rang = bonus.Rang
            };
            return View(bonusView);
        }

        // POST: Bonuses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Rang,Name,Description,DaysToReset,Price,Enabled")] Bonus bonusView)
        {
            if (id != bonusView.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bonusView);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BonusExists(bonusView.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                var requests = from b in _context.BonRequests select b;
                requests = requests.Where(r => r.BonusId == bonusView.Id);
                var req = requests.Where(r => r.Status == "Pending").ToList();
                foreach(var r in req)
                {
                    r.Price = bonusView.Price;
                    _context.Update(r);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bonusView);
        }

        // GET: Bonuses/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonus = await _context.Bonuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bonus == null)
            {
                return NotFound();
            }
            BonusViewModel bonusView = new()
            {
                Id = bonus.Id,
                Name = bonus.Name,
                Price = bonus.Price,
                Enabled = bonus.Enabled,
                DaysToReset = bonus.DaysToReset,
                Description = bonus.Description,
                Rang = bonus.Rang
            };
            return View(bonusView);
        }

        // POST: Bonuses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bonus = await _context.Bonuses.FindAsync(id);
            _context.Bonuses.Remove(bonus);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BonusExists(int id)
        {
            return _context.Bonuses.Any(e => e.Id == id);
        }
    }
}
