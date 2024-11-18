namespace SpecialistService.API.Contracts
{
    public class UserRegisteredEvent
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
