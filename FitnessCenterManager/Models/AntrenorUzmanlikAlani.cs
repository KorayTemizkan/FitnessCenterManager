namespace FitnessCenterManager.Models
{
    public class AntrenorUzmanlikAlani
    {
        public int AntrenorId { get; set; }
        public Antrenor Antrenor { get; set; }
        public int UzmanlikAlaniId { get; set; }
        public UzmanlikAlani UzmanlikAlani { get; set; }
    }
}