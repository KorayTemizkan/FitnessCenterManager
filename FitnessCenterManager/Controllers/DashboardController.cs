using FitnessCenterManager.Data;
using FitnessCenterManager.Models;
using FitnessCenterManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManager.Controllers
{
    [Authorize(Roles = "Admin,Uye")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext ctx, UserManager<IdentityUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var viewModel = new DashboardViewModel();

            if (user != null)
            {
                viewModel.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                viewModel.IsUye = await _userManager.IsInRoleAsync(user, "Uye");
                viewModel.CurrentUserEmail = user.Email;
                viewModel.CurrentUserRoles = await _userManager.GetRolesAsync(user);
            }

            viewModel.Hizmetler = await _ctx.Hizmetler.ToListAsync();
            viewModel.SporSalonu = await _ctx.SporSalonlari.FirstOrDefaultAsync();

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<JsonResult> TumAntrenorler(CancellationToken cancellationToken)
        {
            var antrenorler = await _ctx.Antrenorler
                .Include(a => a.AntrenorUzmanlikAlanlari)
                    .ThenInclude(au => au.UzmanlikAlani)
                .OrderBy(a => a.AntrenorAd)
                .Select(a => new
                {
                    antrenorAd = a.AntrenorAd,
                    antrenorSoyad = a.AntrenorSoyad,
                    uzmanlikAlanlari = string.Join(", ", a.AntrenorUzmanlikAlanlari.Select(au => au.UzmanlikAlani.Ad))
                })
                .ToListAsync(cancellationToken);

            return Json(antrenorler);
        }
    }
}
