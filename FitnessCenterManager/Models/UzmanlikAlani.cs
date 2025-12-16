using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManager.Models
{
    public class UzmanlikAlani
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Uzmanlik alani adi zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Uzmanlik Alani")]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Aciklama")]
        public string? Aciklama { get; set; }

        // Navigation properties
        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        public ICollection<AntrenorUzmanlikAlani> AntrenorUzmanlikAlanlari { get; set; } = new List<AntrenorUzmanlikAlani>();
    }
}
