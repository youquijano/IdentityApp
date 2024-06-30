using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Account
{
    public class ConfirmEmailDto
    {
        [Required]
        public string Token { get; set; }
        [Required]
        [RegularExpression("^([\\w\\-\\.]+)@((\\[([0-9]{1,3}\\.){3}[0-9]{1,3}\\])|(([\\w\\-]+\\.)+)([a-zA-Z]{2,4}))$", ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
    }
}
