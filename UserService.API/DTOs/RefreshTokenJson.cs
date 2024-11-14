using System.ComponentModel.DataAnnotations;

namespace UserService.API.DTOs
{
    public class RefreshTokenJson
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
