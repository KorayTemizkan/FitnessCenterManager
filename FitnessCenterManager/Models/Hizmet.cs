using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManager.Models
{
    public class Hizmet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adý zorunludur.")]
        [StringLength(80, ErrorMessage = "Hizmet adý en fazla 80 karakter olabilir.")]
        [RegularExpression(
       @"^[a-zA-ZçÇðÐýÝöÖþÞüÜ\s'-]+$",
       ErrorMessage = "Hizmet adý sadece harflerden oluþmalýdýr."
   )]
        [Display(Name = "Hizmet Adý")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Açýklama zorunludur.")]
        [StringLength(300, ErrorMessage = "Açýklama en fazla 300 karakter olabilir.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Açýklama")]
        public string Aciklama { get; set; } = string.Empty;


        [Range(0, 10000, ErrorMessage = "Ücret 0 ile 10.000 aralýðýnda olmalýdýr.")]
        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; }

        // Uzmanlik Alani iliskisi (opsiyonel)
        [Display(Name = "Uzmanlýk Alaný")]
        public int? UzmanlikAlaniId { get; set; }

        [ForeignKey("UzmanlikAlaniId")]
        public UzmanlikAlani? UzmanlikAlani { get; set; }

        public ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; } = new List<AntrenorHizmet>();
    }
}