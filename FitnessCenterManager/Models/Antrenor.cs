using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManager.Models
{
    public class Antrenor
    {
        public int AntrenorId { get; set; }

        [Required]
        [StringLength(60)]
        [RegularExpression(@"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s]+$",
            ErrorMessage = "Ad sadece harflerden oluşmalıdır.")]
        [Display(Name = "Ad")]
        public string AntrenorAd { get; set; } = string.Empty;


        [Required]
        [StringLength(60)]
        [RegularExpression(@"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s]+$",
         ErrorMessage = "Soyad sadece harflerden oluşmalıdır.")]
        [Display(Name = "Soyad")]
        public string AntrenorSoyad { get; set; } = string.Empty;


        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [DataType(DataType.MultilineText)]
        public string? Aciklama { get; set; }

        // Navigation properties
        public ICollection<AntrenorUzmanlikAlani> AntrenorUzmanlikAlanlari { get; set; } = new List<AntrenorUzmanlikAlani>();
        public ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();
        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        // Helper property for display
        public string TamAd => $"{AntrenorAd} {AntrenorSoyad}";
    }
}