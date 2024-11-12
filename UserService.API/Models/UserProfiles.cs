using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.API.Models
{
    public class UserProfiles
    {
        public int Id { get; set; }
        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public Users User { get; set; }
        public string? FirstName {  get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber {  get; set; }
        public string? Address { get; set; }
    }
}
