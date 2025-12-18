using FitnessCenterManager.Models;

namespace FitnessCenterManager.Models.ViewModels
{
    public class AntrenorEditViewModel
    {
        public int AntrenorId { get; set; }
        public string AntrenorAd { get; set; } = string.Empty;
        public string AntrenorSoyad { get; set; } = string.Empty;
        public List<int> SeciliUzmanliklar { get; set; } = new();
        public List<int> SeciliHizmetler { get; set; } = new();
        public List<UzmanlikAlani> Uzmanliklar { get; set; } = new();
        public List<Hizmet> Hizmetler { get; set; } = new();
    }
}
