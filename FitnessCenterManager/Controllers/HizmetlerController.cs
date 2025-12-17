using FitnessCenterManager.Data;
using FitnessCenterManager.Models;
using FitnessCenterManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HizmetlerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HizmetlerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hizmetler
        public async Task<IActionResult> Index()
        {
            var hizmetler = await _context.Hizmetler
                .Include(h => h.UzmanlikAlani)
                .OrderBy(h => h.Ad)
                .ToListAsync();

            var viewModel = new HizmetlerIndexViewModel
            {
                Hizmetler = hizmetler
            };
            return View(viewModel);
        }

        // GET: Hizmetler/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.UzmanlikAlanlari = new SelectList(
                await _context.UzmanlikAlanlari.OrderBy(u => u.Ad).ToListAsync(),
                "Id",
                "Ad"
            );
            return View(new Hizmet());
        }

        // POST: Hizmetler/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hizmet hizmet)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UzmanlikAlanlari = new SelectList(
                    await _context.UzmanlikAlanlari.OrderBy(u => u.Ad).ToListAsync(),
                    "Id",
                    "Ad",
                    hizmet.UzmanlikAlaniId
                );
                return View(hizmet);
            }

            _context.Hizmetler.Add(hizmet);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: Hizmetler/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler.FirstOrDefaultAsync(m => m.Id == id);
            if (hizmet == null) return NotFound();

            ViewBag.UzmanlikAlanlari = new SelectList(
                await _context.UzmanlikAlanlari.OrderBy(u => u.Ad).ToListAsync(),
                "Id",
                "Ad",
                hizmet.UzmanlikAlaniId
            );

            return View(hizmet);
        }

        // POST: Hizmetler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hizmet hizmet)
        {
            if (id != hizmet.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.UzmanlikAlanlari = new SelectList(
                    await _context.UzmanlikAlanlari.OrderBy(u => u.Ad).ToListAsync(),
                    "Id",
                    "Ad",
                    hizmet.UzmanlikAlaniId
                );
                return View(hizmet);
            }

            try
            {
                _context.Update(hizmet);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HizmetExists(hizmet.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Hizmetler/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler
                .Include(h => h.UzmanlikAlani)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hizmet == null) return NotFound();

            return View(hizmet);
        }

        // POST: Hizmetler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet != null)
            {
                _context.Hizmetler.Remove(hizmet);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool HizmetExists(int id)
        {
            return _context.Hizmetler.Any(e => e.Id == id);
        }
    }
}
