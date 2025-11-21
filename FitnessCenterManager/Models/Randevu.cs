using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManager.Models
{
    public class Randevu
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen bir antrenör seçin.")]
        public int AntrenorId { get; set; }
        public Antrenor? Antrenor { get; set; }

        [Required(ErrorMessage = "Lütfen bir hizmet seçin.")]
        public int HizmetId { get; set; }
        public Hizmet? Hizmet { get; set; }

        [Required(ErrorMessage = "Lütfen bir randevu tarihi seçin.")]
        public DateTime RandevuTarihi { get; set; }

        [Required]
        public string UyeId { get; set; }
        public IdentityUser? Uye { get; set; }
        public bool Onaylandi { get; set; }
        public DateTime OlusturulmaZamani { get; set; }
    }
}