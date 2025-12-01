using Microsoft.AspNetCore.Mvc;
using FitnessCenterManager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCenterManager.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AntrenorlerController : ControllerBase
    {
        private readonly ApplicationDbContext _ctx;
        public AntrenorlerController(ApplicationDbContext ctx) => _ctx = ctx;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var antrenorler = await _ctx.Antrenorler
                .Include(a => a.AntrenorUzmanlikAlanlari)
                    .ThenInclude(au => au.UzmanlikAlani)
                .OrderBy(a => a.AntrenorAd)
                .Select(a => new
                {
                    a.AntrenorId,
                    a.AntrenorAd,
                    a.AntrenorSoyad,
                    uzmanlikAlanlari = string.Join(", ", a.AntrenorUzmanlikAlanlari.Select(au => au.UzmanlikAlani.Ad))
                })
                .ToListAsync();

            return Ok(antrenorler);
        }

        [HttpGet("availableSlots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] DateTime date)
        {
            var onlyDate = date.Date;

            // 1. Tüm Slotlarý Oluþtur
            var allSlots = Enumerable.Range(8, 12)
                .Select(hour => new
                {
                    Baslangic = onlyDate.AddHours(hour),
                    Bitis = onlyDate.AddHours(hour + 1),
                    Display = $"{hour:D2}:00 - {hour + 1:D2}:00"
                })
                .ToList();

            // 2. Veritabanýndan Dolu Randevularý Çek (UTC aralýðý ile)
            var searchStartUtc = onlyDate.AddDays(-1).ToUniversalTime();
            var searchEndUtc = onlyDate.AddDays(2).ToUniversalTime();

            var busyRandevular = await _ctx.Randevular
     .Include(r => r.Hizmet)
     .Where(r => r.RandevuTarihi >= searchStartUtc && r.RandevuTarihi < searchEndUtc)
     .Select(r => new
     {
         r.AntrenorId,
         RandevuBaslangicUTC = r.RandevuTarihi,
         HizmetSuresi = 60 // Her randevu 1 saat (60 dakika)
     })
     .ToListAsync();

            var allAntrenorler = await _ctx.Antrenorler
                .Include(a => a.AntrenorUzmanlikAlanlari)
                    .ThenInclude(au => au.UzmanlikAlani)
                .Select(a => new 
                { 
                    a.AntrenorId, 
                    a.AntrenorAd, 
                    a.AntrenorSoyad, 
                    UzmanlikAlanlari = string.Join(", ", a.AntrenorUzmanlikAlanlari.Select(au => au.UzmanlikAlani.Ad))
                })
                .ToListAsync();

            // 3. Her Slotun Durumunu (Dolu/Boþ) Hesapla
            var resultData = allAntrenorler.Select(antrenor =>
            {
                var antrenorRandevulari = busyRandevular
                    .Where(r => r.AntrenorId == antrenor.AntrenorId)
                    .Select(r => new
                    {
                        BaslangicTR = r.RandevuBaslangicUTC.AddHours(3),
                        BitisTR = r.RandevuBaslangicUTC.AddHours(3).AddMinutes(r.HizmetSuresi)
                    })
                    .Where(r => r.BaslangicTR.Date == onlyDate)
                    .ToList();

                // Slotlarý filtrelemek yerine hepsini dönüyoruz ama 'IsAvailable' bilgisi ekliyoruz
                var slotStatuses = allSlots.Select(slot => {
                    var isBusy = antrenorRandevulari.Any(busy =>
                        slot.Baslangic < busy.BitisTR && slot.Bitis > busy.BaslangicTR
                    );

                    return new
                    {
                        slot.Baslangic,
                        slot.Display,
                        IsAvailable = !isBusy // Doluysa False, Boþsa True
                    };
                }).ToList();

                return new
                {
                    antrenor.AntrenorId,
                    antrenor.AntrenorAd,
                    antrenor.AntrenorSoyad,
                    antrenor.UzmanlikAlanlari,
                    Slots = slotStatuses
                };
            })
            .OrderBy(a => a.AntrenorAd)
            .ToList();

            return Ok(resultData);
        }
    }
}
