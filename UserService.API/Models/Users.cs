using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.API.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        public string Username {  get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        [ForeignKey("RoleId")]
        public int RoleId {  get; set; }
        public Role Role {  get; set; } 
        public DateTime Created { get; set; }

    }
}
