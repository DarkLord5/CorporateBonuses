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

        private async Task<List<RequestsViewModel>> SearchingAsync(string param)
        {
            var requests = from br in _context.BonRequests select br;
            var req = requests.Where(r => r.Status == param).ToList();
            List<RequestsViewModel> model = new();
            foreach (BonRequest request in req)
            {
                var data = new RequestsViewModel
                {
                    Requests = request,
                    Users = await _userManager.FindByIdAsync(request.UserId),
                    Bonuses = await _context.Bonuses.FindAsync(request.BonusId)
                };
                model.Add(data);
            }
            return model;
        }


        // GET: BonRequests
        public async Task<IActionResult> IndexAsync()
        {
            var model = await SearchingAsync("Pending");
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
            var model = await SearchingAsync("Pending");
            return View(model);
        }

        //// GET: BonRequests/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var bonRequest = await _context.BonRequests
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (bonRequest == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(bonRequest);
        //}

        // GET: BonRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonRequest = await _context.BonRequests.FindAsync(id);
            if (bonRequest == null)
            {
                return NotFound();
            }
            return View(bonRequest);
        }

        // POST: BonRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BonusId,UserId,Status,ApproveDate")] BonRequest bonRequest)
        {
            if (id != bonRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bonRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BonRequestExists(bonRequest.Id))
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
            return View(bonRequest);
        }

        // GET: BonRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bonRequest = await _context.BonRequests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bonRequest == null)
            {
                return NotFound();
            }

            return View(bonRequest);
        }

        // POST: BonRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bonRequest = await _context.BonRequests.FindAsync(id);
            _context.BonRequests.Remove(bonRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BonRequestExists(int id)
        {
            return _context.BonRequests.Any(e => e.Id == id);
        }
    }
}
