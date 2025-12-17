using FitnessCenterManager.Data;
using FitnessCenterManager.Models;
using FitnessCenterManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitnessCenterManager.Controllers
{
    public class RandevularMvcController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RandevularMvcController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Randevular
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

            IQueryable<Randevu> query = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .OrderByDescending(r => r.RandevuTarihi);

            // Admin degilse sadece kendi randevularini gorsun
            if (!isAdmin)
            {
                query = query.Where(r => r.UyeId == user!.Id);
            }

            var randevular = await query.ToListAsync();

            ViewBag.IsAdmin = isAdmin;
            return View(randevular);
        }

        private async Task<RandevuCreateViewModel> PrepareViewModelAsync()
        {
            var viewModel = new RandevuCreateViewModel();

            viewModel.HizmetlerList = new SelectList(
                await _context.Hizmetler.OrderBy(h => h.Ad).ToListAsync(),
                "Id", "Ad");

            var antrenorler = await _context.Antrenorler
                .OrderBy(a => a.AntrenorAd)
                .Select(a => new { a.AntrenorId, FullName = a.AntrenorAd + " " + a.AntrenorSoyad })
                .ToListAsync();
            viewModel.AntrenorlerList = new SelectList(antrenorler, "AntrenorId", "FullName");

            // Saat araliklari (08:00 - 22:00)
            for (int i = 8; i < 22; i++)
            {
                viewModel.SaatAraliklari.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = $"{i:D2}:00 - {i + 1:D2}:00"
                });
            }

            return viewModel;
        }

        // GET: Randevular/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = await PrepareViewModelAsync();
            return View(viewModel);
        }

        // POST: Randevular/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RandevuCreateViewModel viewModel)
        {
            // Kullanici atamasi
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "Identity" });
            viewModel.Randevu.UyeId = userId;

            // Tarih ve saat birlestirme
            DateTime birlesmisTarih = viewModel.SecilenTarih.Date.AddHours(viewModel.SecilenSaat);

            // Gecmis zaman kontrolu
            if (birlesmisTarih < DateTime.Now)
            {
                ModelState.AddModelError("SecilenSaat", "Gecmis bir zaman dilimi secemezsiniz.");
                var vm = await PrepareViewModelAsync();
                vm.Randevu = viewModel.Randevu;
                vm.SecilenTarih = viewModel.SecilenTarih;
                vm.SecilenSaat = viewModel.SecilenSaat;
                return View(vm);
            }

            // Antrenör ve hizmet uyumluluðunu kontrol et
            var antrenorHizmetUyumluMu = await _context.AntrenorHizmetleri
                .AnyAsync(ah => ah.AntrenorId == viewModel.Randevu.AntrenorId &&
                                ah.HizmetId == viewModel.Randevu.HizmetId);

            if (!antrenorHizmetUyumluMu)
            {
                ModelState.AddModelError("Randevu.HizmetId", "Seçilen antrenör bu hizmeti veremiyor.");
                var vm = await PrepareViewModelAsync();
                vm.Randevu = viewModel.Randevu;
                vm.SecilenTarih = viewModel.SecilenTarih;
                vm.SecilenSaat = viewModel.SecilenSaat;
                return View(vm);
            }
            // Cakisma kontrolu
            var slotBaslangicUTC = birlesmisTarih.AddHours(-3);
            var slotBitisUTC = slotBaslangicUTC.AddHours(1);

            var cakismaVarMi = await _context.Randevular
                .AnyAsync(r => r.AntrenorId == viewModel.Randevu.AntrenorId &&
                               r.RandevuTarihi < slotBitisUTC &&
                               r.RandevuTarihi.AddMinutes(60) > slotBaslangicUTC);

            if (cakismaVarMi)
            {
                ModelState.AddModelError("SecilenSaat", "Secilen antrenor bu saat araliginda dolu.");
                var vm = await PrepareViewModelAsync();
                vm.Randevu = viewModel.Randevu;
                vm.SecilenTarih = viewModel.SecilenTarih;
                vm.SecilenSaat = viewModel.SecilenSaat;
                return View(vm);
            }

            // Kayit
            viewModel.Randevu.RandevuTarihi = slotBaslangicUTC;
            viewModel.Randevu.Onaylandi = false;
            viewModel.Randevu.OlusturulmaZamani = DateTime.UtcNow;

            _context.Randevular.Add(viewModel.Randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Randevular/Update/5
        [Authorize(Roles = "Admin,Uye")]
        public async Task<IActionResult> Update(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

            if (!isAdmin && randevu.UyeId != user!.Id)
                return Forbid();

            var vm = await PrepareViewModelAsync();

            vm.Randevu = randevu;
            vm.SecilenTarih = randevu.RandevuTarihi.AddHours(3).Date;
            vm.SecilenSaat = randevu.RandevuTarihi.AddHours(3).Hour;

            return View("Edit", vm);

        }


        // POST: Randevular/Update/5
        [HttpPost]
        [Authorize(Roles = "Admin,Uye")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, RandevuCreateViewModel viewModel)
        {
            // Mevcut randevuyu DB’den al
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

            //  Yetki kontrolü
            if (!isAdmin && randevu.UyeId != user!.Id)
                return Forbid();

            //  Tarih + saat birlestir
            DateTime secilenTarihSaat = viewModel.SecilenTarih.Date
                .AddHours(viewModel.SecilenSaat);

            if (secilenTarihSaat < DateTime.Now)
            {
                ModelState.AddModelError("SecilenSaat", "Geçmiþ bir zaman seçemezsiniz.");
                var vm = await PrepareViewModelAsync();
                vm.Randevu = randevu;            
                vm.SecilenTarih = viewModel.SecilenTarih;
                vm.SecilenSaat = viewModel.SecilenSaat;
                return View("Edit", vm);
            }
            // Antrenör ve hizmet uyumluluðunu kontrol et
            var antrenorHizmetUyumluMu = await _context.AntrenorHizmetleri
                .AnyAsync(ah => ah.AntrenorId == viewModel.Randevu.AntrenorId &&
                                ah.HizmetId == viewModel.Randevu.HizmetId);

            if (!antrenorHizmetUyumluMu)
            {
                ModelState.AddModelError("Randevu.HizmetId", "Seçilen antrenör bu hizmeti veremiyor.");
                var vm = await PrepareViewModelAsync();
                vm.Randevu = randevu;
                vm.SecilenTarih = viewModel.SecilenTarih;
                vm.SecilenSaat = viewModel.SecilenSaat;
                return View("Edit", vm);
            }

            // 4UTC dönüþümü
            var baslangicUTC = secilenTarihSaat.AddHours(-3);
            var bitisUTC = baslangicUTC.AddHours(1);

            //  Çakýþma kontrolü (kendisi haric)
            var cakismaVarMi = await _context.Randevular.AnyAsync(r =>
                r.Id != randevu.Id &&
                r.AntrenorId == viewModel.Randevu.AntrenorId &&
                r.RandevuTarihi < bitisUTC &&
                r.RandevuTarihi.AddMinutes(60) > baslangicUTC
            );

            if (cakismaVarMi)
            {
                ModelState.AddModelError("SecilenSaat", "Bu saat dolu.");
                var vm = await PrepareViewModelAsync();
                vm.Randevu = randevu;           
                vm.SecilenTarih = viewModel.SecilenTarih;
                vm.SecilenSaat = viewModel.SecilenSaat;
                return View("Edit", vm);

            }

            //  SADECE GÜNCELLE
            randevu.AntrenorId = viewModel.Randevu.AntrenorId;
            randevu.HizmetId = viewModel.Randevu.HizmetId;
            randevu.RandevuTarihi = baslangicUTC;
            randevu.Onaylandi = false; // tekrar onaylansýn
            _context.Update(randevu);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        // POST: Randevular/Approve/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Onaylandi = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unapprove(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound("Randevu bulunamadý.");

            randevu.Onaylandi = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Randevular/Delete/5    
        [HttpPost]
        [Authorize(Roles = "Admin,Uye")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
