using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpecialistService.API.Models
{
    public class Skills
    {
        [Key]
        public int Id { get; set; }
        public string SkillName { get; set; }
        public int SpecialistsId { get; set; }
        public Specialists? Specialists { get; set; }
    }
}
