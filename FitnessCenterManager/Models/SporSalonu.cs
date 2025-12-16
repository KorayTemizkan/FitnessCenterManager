using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManager.Models
{
    public class SporSalonu
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Salon adı zorunludur.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Salon adı 3-50 karakter arası olmalıdır.")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Salon adresi zorunludur.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Salon adı 3-50 karakter arası olmalıdır.")]
        public string? Adres { get; set; }

        [StringLength(100, ErrorMessage = "Telefon en fazla 100 karakter olabilir.")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Telefon numarası 10 haneli olmalıdır. Örnek: 5343936207")]
        public string? Telefon { get; set; }

        [Required(ErrorMessage = "Açılış saati zorunludur.")]
        [Display(Name = "Açılış Saati")]
        public TimeSpan AcilisSaati { get; set; }

        [Required(ErrorMessage = "Kapanış saati zorunludur.")]
        [Display(Name = "Kapanış Saati")]
        [CustomValidation(typeof(SporSalonu), nameof(ValidateKapanisSaati))]
        public TimeSpan KapanisSaati { get; set; }

        // Kapanış saati açılıştan sonra olmalı
        public static ValidationResult? ValidateKapanisSaati(TimeSpan kapanisSaati, ValidationContext context)
        {
            var instance = context.ObjectInstance as SporSalonu;
            if (instance != null && kapanisSaati <= instance.AcilisSaati)
            {
                return new ValidationResult("Kapanış saati, açılış saatinden sonra olmalıdır.");
            }
            return ValidationResult.Success;
        }
    }
}