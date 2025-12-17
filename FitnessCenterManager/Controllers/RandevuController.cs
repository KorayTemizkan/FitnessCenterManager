using FitnessCenterManager.Data;
using FitnessCenterManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManager.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    // Sadece Adminler bu API'yi kullanabilsin (Güvenlik)
    [Authorize(Roles = "Admin")]
    public class RandevularController : ControllerBase
    {
        private readonly ApplicationDbContext _ctx;

        public RandevularController(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        // 1. Tüm Randevuları Getir (Onay durumuyla birlikte)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _ctx.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye) // Üye ismini de göstermek istersen
                .OrderByDescending(r => r.RandevuTarihi) // En yeni en üstte
                .Select(r => new
                {
                    r.Id,
                    Tarih = r.RandevuTarihi, // JS tarafında formatlanacak
                    Antrenor = r.Antrenor.AntrenorAd + " " + r.Antrenor.AntrenorSoyad,
                    Hizmet = r.Hizmet.Ad,
                    Uye = r.Uye.UserName, // Veya Email
                    Onaylandi = r.Onaylandi
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] Randevu model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var randevu = await _ctx.Randevular.FindAsync(model.Id);
            if (randevu == null) return NotFound("Randevu bulunamadı.");

            randevu.Onaylandi = true;
            await _ctx.SaveChangesAsync();

            return Ok(new { message = "Randevu başarıyla onaylandı." });
        }
    }
}