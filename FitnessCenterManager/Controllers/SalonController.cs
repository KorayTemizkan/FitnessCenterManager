using FitnessCenterManager.Data;
using FitnessCenterManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SalonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Salon/Edit
        public async Task<IActionResult> Edit()
        {
            var salon = await _context.SporSalonlari.FirstOrDefaultAsync();
            if (salon == null) salon = new SporSalonu();
            return View(salon);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SporSalonu salon)
        {
            if (!ModelState.IsValid) return View(salon);

            var mevcutSalon = await _context.SporSalonlari.FirstOrDefaultAsync();

            if (mevcutSalon == null)
            {
                _context.SporSalonlari.Add(salon);
                await _context.SaveChangesAsync();
            }
            else
            {
                mevcutSalon.Ad = salon.Ad;
                mevcutSalon.Adres = salon.Adres;
                mevcutSalon.Telefon = salon.Telefon;
                mevcutSalon.AcilisSaati = salon.AcilisSaati;
                mevcutSalon.KapanisSaati = salon.KapanisSaati;

                // Uyumsuz randevularý temizleme
                var silinecekRandevular = await _context.Randevular
                    .Where(r => r.RandevuTarihi.TimeOfDay > salon.KapanisSaati)
                    .ToListAsync();

                if (silinecekRandevular.Any())
                {
                    _context.Randevular.RemoveRange(silinecekRandevular);
                    TempData["Message"] = $"Saat deðiþikliði nedeniyle {silinecekRandevular.Count} adet uyumsuz randevu silindi.";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
