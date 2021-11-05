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

namespace CorporateBonuses.Controllers
{
    public class BonusesController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;

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
                BonusViewModel bView = new BonusViewModel(bonus);
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
        

        public async Task<IActionResult> UserList()
        {
            string u = User.Identity.Name;
            User user = await _userManager.FindByNameAsync(u);
            return View(Filtration(user));
        }

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
            }
            _context.Add(req);
            await _context.SaveChangesAsync();
            return View(Filtration(user));
        }
        // GET: Bonuses
        public async Task<IActionResult> Index()
        {
            var bonuses = await _context.Bonuses.ToListAsync();
            return View(ToViewModel(bonuses));
        }

        // GET: Bonuses/Details/5
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
            BonusViewModel bonusView = new(bonus);
            return View(bonusView);
        }

        // GET: Bonuses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Bonuses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BonusViewModel bonusView)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bonusView.Bonus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bonusView);
        }

        // GET: Bonuses/Edit/5
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
            BonusViewModel bonusView = new(bonus);
            return View(bonusView);
        }

        // POST: Bonuses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Rang,Name,Description,DaysToReset,Price,Enabled")] BonusViewModel bonusView)
        {
            if (id != bonusView.Bonus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bonusView.Bonus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BonusExists(bonusView.Bonus.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                var requests = from b in _context.BonRequests select b;
                requests = requests.Where(r => r.BonusId == bonusView.Bonus.Id);
                var req = requests.Where(r => r.Status == "Pending").ToList();
                foreach(var r in req)
                {
                    r.Price = bonusView.Bonus.Price;
                    _context.Update(r);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bonusView.Bonus);
        }

        // GET: Bonuses/Delete/5
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
            BonusViewModel bonusView = new(bonus);
            return View(bonusView);
        }

        // POST: Bonuses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
