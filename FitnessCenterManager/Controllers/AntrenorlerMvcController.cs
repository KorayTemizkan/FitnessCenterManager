using FitnessCenterManager.Data;
using FitnessCenterManager.Models;
using FitnessCenterManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AntrenorlerMvcController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorlerMvcController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Antrenorler
        public async Task<IActionResult> Index()
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.AntrenorUzmanlikAlanlari)
                    .ThenInclude(au => au.UzmanlikAlani)
                .OrderBy(a => a.AntrenorAd)
                .Select(a => new AntrenorListItemViewModel
                {
                    AntrenorId = a.AntrenorId,
                    AntrenorAd = a.AntrenorAd,
                    AntrenorSoyad = a.AntrenorSoyad,
                    UzmanlikAlanlariText = string.Join(", ", a.AntrenorUzmanlikAlanlari.Select(au => au.UzmanlikAlani.Ad))
                })
                .ToListAsync();

            var viewModel = new AntrenorlerIndexViewModel
            {
                Antrenorler = antrenorler
            };
            return View(viewModel);
        }

        // GET: Antrenorler/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new AntrenorCreateViewModel
            {
                Uzmanliklar = await _context.UzmanlikAlanlari.OrderBy(u => u.Ad).ToListAsync(),
                Hizmetler = await _context.Hizmetler.OrderBy(h => h.Ad).ToListAsync()
            };
            return View(viewModel);
        }
        // AntrenorlerMvcController.cs içindeki POST Create metodunu deðiþtirin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AntrenorCreateViewModel viewModel)
        {
            // Liste doldurma (baþarýsýz validasyonlarda view'a geri dönebilmek için)
            viewModel.Uzmanliklar = await _context.UzmanlikAlanlari.OrderBy(u => u.Ad).ToListAsync();
            viewModel.Hizmetler = await _context.Hizmetler.OrderBy(h => h.Ad).ToListAsync();

            if (!ModelState.IsValid)
                return View(viewModel);

            // (Opsiyonel) seçilen hizmetlerin seçilen uzmanlýklarla iliþkili olup olmadýðýný kontrol et
            var allowedHizmetIds = await _context.Hizmetler
                .Where(h => viewModel.SeciliUzmanliklar.Contains(h.UzmanlikAlaniId ?? 0))
                .Select(h => h.Id)
                .ToListAsync();

            if (viewModel.SeciliHizmetler.Except(allowedHizmetIds).Any())
            {
                ModelState.AddModelError("", "Seçtiðiniz hizmetlerden bazýlarý seçtiðiniz uzmanlýk alanlarýyla iliþkili deðil.");
                return View(viewModel);
            }

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var antrenor = viewModel.Antrenor;
                _context.Antrenorler.Add(antrenor);
                await _context.SaveChangesAsync(); // AntrenorId alýnýr

                if (viewModel.SeciliUzmanliklar?.Any() == true)
                {
                    var au = viewModel.SeciliUzmanliklar
                        .Select(id => new AntrenorUzmanlikAlani { AntrenorId = antrenor.AntrenorId, UzmanlikAlaniId = id });
                    _context.AntrenorUzmanlikAlanlari.AddRange(au);
                }

                if (viewModel.SeciliHizmetler?.Any() == true)
                {
                    var ah = viewModel.SeciliHizmetler
                        .Select(id => new AntrenorHizmet { AntrenorId = antrenor.AntrenorId, HizmetId = id });
                    _context.AntrenorHizmetleri.AddRange(ah);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return RedirectToAction("Index", "Dashboard");
            }
            catch
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "Kaydetme sýrasýnda hata oluþtu.");
                return View(viewModel);
            }
        }

        // GET: Antrenorler/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorUzmanlikAlanlari)
                .Include(a => a.AntrenorHizmetleri)
                .FirstOrDefaultAsync(m => m.AntrenorId == id);
            
            if (antrenor == null) return NotFound();

            var viewModel = new AntrenorEditViewModel
            {
                AntrenorId = antrenor.AntrenorId,
                AntrenorAd = antrenor.AntrenorAd,
                AntrenorSoyad = antrenor.AntrenorSoyad,
                SeciliUzmanliklar = antrenor.AntrenorUzmanlikAlanlari.Select(au => au.UzmanlikAlaniId).ToList(),
                SeciliHizmetler = antrenor.AntrenorHizmetleri.Select(ah => ah.HizmetId).ToList(),
                Uzmanliklar = await _context.UzmanlikAlanlari.OrderBy(u => u.Ad).ToListAsync(),
                Hizmetler = await _context.Hizmetler.OrderBy(h => h.Ad).ToListAsync()
            };

            return View(viewModel);
        }
        // POST: Antrenorler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AntrenorEditViewModel viewModel)
        {
            if (id != viewModel.AntrenorId) return NotFound();

            // --- ÝLÝÞKÝ KONTROLÜ EKLENDÝ ---
            // Seçilen uzmanlýk alanlarýna baðlý hizmet ID'lerini bul
            var allowedHizmetIds = await _context.Hizmetler
                .Where(h => viewModel.SeciliUzmanliklar.Contains(h.UzmanlikAlaniId ?? 0))
                .Select(h => h.Id)
                .ToListAsync();

            // Seçilen hizmetlerden, izinli olmayanlarý bul
            var invalidHizmetIds = viewModel.SeciliHizmetler.Except(allowedHizmetIds).ToList();

            if (invalidHizmetIds.Any())
            {
                ModelState.AddModelError("", "Seçtiðiniz hizmetlerden bazýlarý, seçtiðiniz uzmanlýk alanlarýyla iliþkili deðil. Lütfen sadece ilgili hizmetleri seçin.");
                // ViewModel'i tekrar doldurup view'a dön
                viewModel.Uzmanliklar = await _context.UzmanlikAlanlari.OrderBy(u => u.Ad).ToListAsync();
                viewModel.Hizmetler = await _context.Hizmetler.OrderBy(h => h.Ad).ToListAsync();
                return View(viewModel);
            }
            // --- ÝLÝÞKÝ KONTROLÜ SONU ---

            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorUzmanlikAlanlari)
                .Include(a => a.AntrenorHizmetleri)
                .FirstOrDefaultAsync(a => a.AntrenorId == id);

            if (antrenor == null) return NotFound();

            // Temel bilgileri guncelle
            antrenor.AntrenorAd = viewModel.AntrenorAd;
            antrenor.AntrenorSoyad = viewModel.AntrenorSoyad;

            // Eski uzmanlik alanlarini sil, yenilerini ekle
            _context.AntrenorUzmanlikAlanlari.RemoveRange(antrenor.AntrenorUzmanlikAlanlari);
            foreach (var uzmanlikId in viewModel.SeciliUzmanliklar)
            {
                _context.AntrenorUzmanlikAlanlari.Add(new AntrenorUzmanlikAlani
                {
                    AntrenorId = id,
                    UzmanlikAlaniId = uzmanlikId
                });
            }

            // Eski hizmetleri sil, yenilerini ekle
            _context.AntrenorHizmetleri.RemoveRange(antrenor.AntrenorHizmetleri);
            foreach (var hizmetId in viewModel.SeciliHizmetler)
            {
                _context.AntrenorHizmetleri.Add(new AntrenorHizmet
                {
                    AntrenorId = id,
                    HizmetId = hizmetId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Antrenorler/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorUzmanlikAlanlari)
                    .ThenInclude(au => au.UzmanlikAlani)
                .FirstOrDefaultAsync(m => m.AntrenorId == id);
            
            if (antrenor == null) return NotFound();

            var viewModel = new AntrenorDeleteViewModel
            {
                AntrenorId = antrenor.AntrenorId,
                AntrenorAd = antrenor.AntrenorAd,
                AntrenorSoyad = antrenor.AntrenorSoyad,
                UzmanlikAlanlariText = string.Join(", ", antrenor.AntrenorUzmanlikAlanlari.Select(au => au.UzmanlikAlani.Ad))
            };

            return View(viewModel);
        }

        // POST: Antrenorler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler
                .Include(a => a.AntrenorUzmanlikAlanlari)
                .Include(a => a.AntrenorHizmetleri)
                .FirstOrDefaultAsync(a => a.AntrenorId == id);

            if (antrenor != null)
            {
                _context.AntrenorUzmanlikAlanlari.RemoveRange(antrenor.AntrenorUzmanlikAlanlari);
                _context.AntrenorHizmetleri.RemoveRange(antrenor.AntrenorHizmetleri);
                _context.Antrenorler.Remove(antrenor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.AntrenorId == id);
        }
    }
}
