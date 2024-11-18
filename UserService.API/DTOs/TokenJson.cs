namespace UserService.API.DTOs
{
    public class TokenJson
    {

        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
