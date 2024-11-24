using SpecialistService.API.Models;

namespace SpecialistService.API.DTOs
{
    public class ProfileJson
    {
        public required List<string> SkillName { get; set; }
        public required string Profession {  get; set; }
        public string? Description { get; set; }
    }
}
