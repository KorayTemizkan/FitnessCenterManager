using FitnessCenterManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessCenterManager.Models.ViewModels
{
    public class AntrenorCreateViewModel
    {
        public Antrenor Antrenor { get; set; } = new();
        public List<UzmanlikAlani> Uzmanliklar { get; set; } = new();
        public List<int> SeciliUzmanliklar { get; set; } = new();
        public List<Hizmet> Hizmetler { get; set; } = new();
        public List<int> SeciliHizmetler { get; set; } = new();
    }
}
