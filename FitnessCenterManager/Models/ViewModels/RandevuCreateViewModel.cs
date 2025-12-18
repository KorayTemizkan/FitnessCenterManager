using FitnessCenterManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessCenterManager.Models.ViewModels
{
    public class RandevuCreateViewModel
    {
        public Randevu Randevu { get; set; } = new();
        public DateTime SecilenTarih { get; set; } = DateTime.Today;
        public int SecilenSaat { get; set; }
        public SelectList? HizmetlerList { get; set; }
        public SelectList? AntrenorlerList { get; set; }
        public List<SelectListItem> SaatAraliklari { get; set; } = new();
    }
}
