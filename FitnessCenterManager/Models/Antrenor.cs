using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManager.Models
{
    public class Antrenor
    {
        public int AntrenorId { get; set; }

        [Required, StringLength(60)]
        public string AntrenorAd { get; set; } = string.Empty;

        [Required, StringLength(60)]
        public string AntrenorSoyad { get; set; } = string.Empty;

        [Display(Name = "Uzmanlık Alanları"), StringLength(300)]
        public string UzmanlikAlanlari { get; set; } = string.Empty; // Virgülle ayrılmış metin

        public ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}