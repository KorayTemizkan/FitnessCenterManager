using FitnessCenterManager.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RandevuApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RandevuApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .Select(r => new {
                    r.Id,
                    r.RandevuTarihi,
                    r.Onaylandi,
                    r.UyeId,
                    r.AntrenorId,
                    r.HizmetId,
                    r.OlusturulmaZamani,
                    Antrenor = r.Antrenor != null ? $"{r.Antrenor.AntrenorAd} {r.Antrenor.AntrenorSoyad}" : null,
                    Hizmet = r.Hizmet != null ? r.Hizmet.Ad : null,
                    Uye = r.Uye != null ? r.Uye.UserName : null
                })
                .OrderByDescending(r => r.RandevuTarihi)
                .ToListAsync();
            return Ok(data);
        }


        // GET: api/RandevuApi/MusaitAntrenorler?tarih=2025-11-20
        [HttpGet("MusaitAntrenorler")]
        public async Task<IActionResult> GetMusaitAntrenorler([FromQuery] DateTime tarih)
        {
            if (tarih == default)
            {
                return BadRequest("Lütfen geçerli bir tarih sağlayın.");
            }

            try
            {
                // Belirtilen tarihte randevusu olan antrenörlerin ID'lerini bul
                var doluAntrenorIdleri = await _context.Randevular
                    .Where(r => r.RandevuTarihi.Date == tarih.Date)
                    .Select(r => r.AntrenorId)
                    .Distinct()
                    .ToListAsync();

                // Randevusu olmayan (müsait) antrenörleri bul
                var musaitAntrenorler = await _context.Antrenorler
                    .Where(a => !doluAntrenorIdleri.Contains(a.AntrenorId))
                    .Select(a => new { a.AntrenorId, Ad = a.AntrenorAd + " " + a.AntrenorSoyad, a.UzmanlikAlanlari })
                    .ToListAsync();

                return Ok(musaitAntrenorler);
            }
            catch (Exception ex)
            {
                // Hata durumunda sunucu hatası döndür
                return StatusCode(500, "Veriler alınırken bir hata oluştu: " + ex.Message);
            }
        }
    }
}