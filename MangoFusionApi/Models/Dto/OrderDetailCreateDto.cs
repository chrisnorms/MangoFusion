using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MangoFusionApi.Models.Dto
{
    public class OrderDetailCreateDto
    {
        [Required]
        public int MenuItemId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public string ItemName { get; set; } = string.Empty;
        [Required]
        public double Price { get; set; }
    }
}
