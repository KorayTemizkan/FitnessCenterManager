using FitnessCenterManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// Dosyanýn en üstüne ekleyin
using FitnessCenterManager.Services; // OpenAiService'in bulunduðu namespace'i ekleyin
namespace FitnessCenterManager.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        private readonly OpenAiService _aiService; // Ýsmi güncelledik
        public AiController(OpenAiService aiService)
        {
            _aiService = aiService;
        }

        public IActionResult Index() => View(new AiViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AiViewModel viewModel)
        {
            byte[]? imageBytes = null;

            if (viewModel.Photo != null && viewModel.Photo.Length > 0)
            {
                if (viewModel.Photo.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("Photo", "Fotoðraf 5MB'dan küçük olmalýdýr.");
                    return View(viewModel);
                }

                using var memoryStream = new MemoryStream();
                await viewModel.Photo.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            viewModel.AiResponse = await _aiService.GetFitnessPlanAsync(
                viewModel.Height ?? "",
                viewModel.Weight ?? "",
                viewModel.Gender ?? "",
                viewModel.Goal ?? "",
                imageBytes);

            return View(viewModel);
        }
    }
}