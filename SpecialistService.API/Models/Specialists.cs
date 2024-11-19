using System.ComponentModel.DataAnnotations;

namespace SpecialistService.API.Models
{
    public class Specialists
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Profession {  get; set; }
        public float? Rating { get; set; }
        public List<Skills>? Skills { get; set; }
    }
}
