using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManager.Models
{
    public class AntrenorUzmanlikAlani
    {
        public int AntrenorId { get; set; }

        [ForeignKey("AntrenorId")]
        public Antrenor Antrenor { get; set; } = null!;

        public int UzmanlikAlaniId { get; set; }

        [ForeignKey("UzmanlikAlaniId")]
        public UzmanlikAlani UzmanlikAlani { get; set; } = null!;
    }
}
