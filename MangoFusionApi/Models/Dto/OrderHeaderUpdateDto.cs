using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MangoFusionApi.Models.Dto
{
    public class OrderHeaderUpdateDto
    {
        [Required]
        public int OrderHeaderId { get; set; }
        public string PickUpName { get; set; } = string.Empty;
        public string PickUpPhoneNumber { get; set; } = string.Empty;
        public string PickUpEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
