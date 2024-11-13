using System.ComponentModel.DataAnnotations;

namespace UserService.API.DTOs
{
    public class RegistrationJson
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public bool IsSpecialist {  get; set; }

    }
}
