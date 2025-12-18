using FitnessCenterManager.Models;

namespace FitnessCenterManager.Models.ViewModels
{
    public class HizmetlerIndexViewModel
    {
        public IList<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
    }
}
