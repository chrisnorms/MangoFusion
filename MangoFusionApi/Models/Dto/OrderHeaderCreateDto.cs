using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MangoFusionApi.Models.Dto
{
    public class OrderHeaderCreateDto
    {
        [Required]
        public string PickUpName { get; set; } = string.Empty;
        [Required]
        public string PickUpPhoneNumber { get; set; } = string.Empty;
        [Required]
        public string PickUpEmail { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
        public double OrderTotal { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalItem { get; set; }

        public List<OrderDetailCreateDto> OrderDetailsDto { get; set; } = new();
    }
}
