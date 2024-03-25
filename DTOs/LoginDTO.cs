using System.ComponentModel.DataAnnotations;

namespace projectUsers.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
