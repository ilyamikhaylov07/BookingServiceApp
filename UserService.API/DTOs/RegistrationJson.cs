namespace UserService.API.DTOs
{
    public class RegistrationJson
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public bool IsSpecialist { get; set; }

    }
}
