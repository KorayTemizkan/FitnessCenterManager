using FitnessCenterManager.Models;

namespace FitnessCenterManager.Models.ViewModels
{
    public class DashboardViewModel
    {
        public IList<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public SporSalonu? SporSalonu { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsUye { get; set; }
        public string? CurrentUserEmail { get; set; }
        public IList<string> CurrentUserRoles { get; set; } = new List<string>();
    }
}
