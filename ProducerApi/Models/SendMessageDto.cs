using System.ComponentModel.DataAnnotations;

namespace ProducerApi.Models
{
    public class SendMessageDto
    {
        [Required(ErrorMessage = "Message is required.")]
        [StringLength(500, ErrorMessage = "Only 500 characters are allowed.")]
        [MinLength(2, ErrorMessage = "Message must be at least 2 characters long.")]
        public string Message { get; set; } = string.Empty;
    }
}
