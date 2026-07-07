using System.ComponentModel.DataAnnotations;

namespace MangoFusionApi.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SpecialTag { get; set; } = string.Empty;
        [Range(1,1000)]
        public double Price { get; set; }
        public string Image { get; set; } = string.Empty;
    }
}
