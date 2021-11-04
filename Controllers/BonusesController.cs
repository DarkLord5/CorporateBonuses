using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CorporateBonuses.Models;
using Microsoft.AspNetCore.Identity;

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

        private IQueryable Filtration(User user)
        {
            var bonuses = from b in _context.Bonuses select b;
            bonuses = bonuses.Where(b => b.Rang <= user.Rang);
            bonuses = bonuses.Where(b => b.Enabled);
            return (bonuses);
        }
        

        public async Task<IActionResult> UserList()
        {
            string u = User.Identity.Name; //
            User user = await _userManager.FindByNameAsync(u);
            return View(Filtration(user));
        }

        [HttpPost]
        public async Task<IActionResult> UserList(int Id)
        {
            string u = User.Identity.Name;
            User user = await _userManager.FindByNameAsync(u);
            Bonus bon = await _context.Bonuses.FindAsync(Id);
            BonRequest req = new() { UserId = user.Id, BonusId = Id,  ApproveDate = DateTime.Today };
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
            return View(await _context.Bonuses.ToListAsync());
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

            return View(bonus);
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
        public async Task<IActionResult> Create(Bonus bonus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bonus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bonus);
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
            return View(bonus);
        }

        // POST: Bonuses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Rang,Name,Description,DaysToReset,Price,Enabled")] Bonus bonus)
        {
            if (id != bonus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bonus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BonusExists(bonus.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bonus);
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

            return View(bonus);
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
