using FitnessCenterManager.Data;
using FitnessCenterManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UzmanlikAlanlariController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UzmanlikAlanlariController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UzmanlikAlanlari
        public async Task<IActionResult> Index()
        {
            var uzmanlikAlanlari = await _context.UzmanlikAlanlari
                .Include(u => u.Hizmetler)
                .OrderBy(u => u.Ad)
                .ToListAsync();
            
            return View(uzmanlikAlanlari);
        }

        // GET: UzmanlikAlanlari/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UzmanlikAlanlari/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UzmanlikAlani uzmanlikAlani)
        {
            if (ModelState.IsValid)
            {
                _context.UzmanlikAlanlari.Add(uzmanlikAlani);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(uzmanlikAlani);
        }

        // GET: UzmanlikAlanlari/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var uzmanlikAlani = await _context.UzmanlikAlanlari.FindAsync(id);
            if (uzmanlikAlani == null) return NotFound();

            return View(uzmanlikAlani);
        }

        // POST: UzmanlikAlanlari/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UzmanlikAlani uzmanlikAlani)
        {
            if (id != uzmanlikAlani.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uzmanlikAlani);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UzmanlikAlaniExists(uzmanlikAlani.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(uzmanlikAlani);
        }

        // GET: UzmanlikAlanlari/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var uzmanlikAlani = await _context.UzmanlikAlanlari
                .Include(u => u.Hizmetler)
                .Include(u => u.AntrenorUzmanlikAlanlari)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (uzmanlikAlani == null) return NotFound();

            return View(uzmanlikAlani);
        }

        // POST: UzmanlikAlanlari/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uzmanlikAlani = await _context.UzmanlikAlanlari
                .Include(u => u.Hizmetler)
                .Include(u => u.AntrenorUzmanlikAlanlari)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (uzmanlikAlani != null)
            {
                // Bagli hizmetlerin UzmanlikAlaniId'sini null yap
                foreach (var hizmet in uzmanlikAlani.Hizmetler)
                {
                    hizmet.UzmanlikAlaniId = null;
                }

                // Antrenor iliskilerini sil
                _context.AntrenorUzmanlikAlanlari.RemoveRange(uzmanlikAlani.AntrenorUzmanlikAlanlari);
                
                _context.UzmanlikAlanlari.Remove(uzmanlikAlani);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UzmanlikAlaniExists(int id)
        {
            return _context.UzmanlikAlanlari.Any(e => e.Id == id);
        }
    }
}
