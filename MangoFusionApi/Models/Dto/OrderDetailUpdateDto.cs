using System.ComponentModel.DataAnnotations;

namespace MangoFusionApi.Models.Dto
{
    public class OrderDetailUpdateDto
    {
        [Required]
        public int OrderDetailId { get; set; }
        [Required]
        [Range(1,5)]
        public int Rating { get; set; }
    }
}
