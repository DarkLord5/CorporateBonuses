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

        public BonusesController(ApplicationContext context, UserManager<User> userManager )
        {
            _context = context;
            _userManager = userManager;
        }

        //Возвращаем список с фильтрованными бонусами
        private List<BonusViewModel> Filtration(User user, string response = "")
        {
            
            var bonuses = _context.Bonuses.Where(b=>b.Enabled).OrderBy(b=>b.Rang).ToList();
            List<BonusViewModel> bonusViews = new();
            foreach (var bonus in bonuses)
            {
                var persbon = _context.PersonalBonuses.Where(pb => pb.UserId == user.Id).Where(pb => bonus.Id == pb.BonusId).Where(pb => pb.EnableDate > DateTime.Today).ToList();
                string answer="Доступен";
                if (user.Rang<bonus.Rang)
                {
                    answer = $"Необходим {bonus.Rang} ранг";
                }else if (persbon.Count!=0)
                {
                    answer = $"Бонус будет доступен {persbon[0].EnableDate}";
                }
                BonusViewModel bView = new()
                {
                    Id = bonus.Id,
                    Name = bonus.Name,
                    Price = bonus.Price,
                    Enabled = bonus.Enabled,
                    DaysToReset = bonus.DaysToReset,
                    Description = bonus.Description,
                    Rang = bonus.Rang,
                    UserRang = user.Rang,
                    EnableString = answer,
                    Answer = response
                };
                bonusViews.Add(bView);
            }

            return bonusViews;
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
            var persbon = _context.PersonalBonuses.Where(pb => pb.UserId == user.Id).Where(pb => bon.Id == pb.BonusId).Where(pb => pb.EnableDate > DateTime.Today).ToList();
            if ((bon.Rang<=user.Rang)&&(persbon.Count==0))
            {
                BonRequest req = new() { UserId = user.Id, BonusId = Id, ApproveDate = DateTime.Today, Price = bon.Price };
                if (bon.Rang > 1)
                {
                    req.Status = "Pending";
                    EmailService emailService = new EmailService();
                    await emailService.SendEmailAsync("noreykoartem@gmail.com", "Запрос от пользователя",$"{user.FirstName} {user.Surname} претендует на бонус {bon.Name}");
                }
                else
                {
                    req.Status = "Approved";
                }

                PersonalBonus persBon = new()
                {
                    UserId = req.UserId,
                    BonusId = req.BonusId,
                    EnableDate = DataCounterService.EnableDate(bon.DaysToReset)
                };

                _context.Add(persBon);
                _context.Add(req);
                await _context.SaveChangesAsync();
                return View(Filtration(user, $"Запрос на получение {bon.Name} был отправлен"));
            }

            return View(Filtration(user, $"Запрос не отправлен."));
        }

        // GET: Bonuses
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var bonuses = await _context.Bonuses.OrderBy(b=>b.Rang).ToListAsync();
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
            return View(bonusViews);
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
                var requests = _context.BonRequests.Where(r => r.BonusId == bonusView.Id).Where(r => r.Status == "Pending").ToList();
                if (!bonusView.Enabled)
                {
                    foreach(var r in requests)
                    {
                        r.Status = "Rejected";
                        _context.Update(r);
                    }
                }

                foreach(var r in requests)
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
