using FitnessCenterManager.Models;

namespace FitnessCenterManager.Models.ViewModels
{
    public class AntrenorlerIndexViewModel
    {
        public IList<AntrenorListItemViewModel> Antrenorler { get; set; } = new List<AntrenorListItemViewModel>();
    }

    public class AntrenorListItemViewModel
    {
        public int AntrenorId { get; set; }
        public string AntrenorAd { get; set; } = string.Empty;
        public string AntrenorSoyad { get; set; } = string.Empty;
        public string UzmanlikAlanlariText { get; set; } = string.Empty;
    }
}
