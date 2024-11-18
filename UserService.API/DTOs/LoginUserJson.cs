namespace UserService.API.DTOs
{
    public class LoginUserJson
    {
        public required string Email {  get; set; }
        public required string Password { get; set; }
    }
}
