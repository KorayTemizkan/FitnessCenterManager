using Microsoft.AspNetCore.Http;

namespace FitnessCenterManager.Models.ViewModels
{
    public class AiViewModel
    {
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? Gender { get; set; }
        public string? Goal { get; set; }
        public IFormFile? Photo { get; set; }

        // AI'dan gelen HTML (Diyet planý + varsa img etiketi) buraya düþecek
        public string? AiResponse { get; set; }
    }
}
