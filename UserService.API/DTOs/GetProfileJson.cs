using System.ComponentModel.DataAnnotations;

namespace UserService.API.DTOs
{
    public class GetProfileJson
    {
        [Required]
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
