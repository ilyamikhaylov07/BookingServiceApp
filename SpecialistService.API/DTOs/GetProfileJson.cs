namespace SpecialistService.API.DTOs
{
    public class GetProfileJson
    {
        public int Id { get; set; }
        public List<string>? SkillName { get; set; }
        public string? Profession { get; set; }
        public string? Description { get; set; }
    }
}
