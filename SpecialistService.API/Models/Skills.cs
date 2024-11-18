using System.ComponentModel.DataAnnotations;

namespace SpecialistService.API.Models
{
    public class Skills
    {
        [Key]
        public int Id { get; set; }
        public string SkillName { get; set; }
        public int SpecialistId { get; set; }
        public Specialists? Specialists { get; set; }
    }
}
