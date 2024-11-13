using System.ComponentModel.DataAnnotations;

namespace UserService.API.DTOs
{
    public class TokenJson
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
