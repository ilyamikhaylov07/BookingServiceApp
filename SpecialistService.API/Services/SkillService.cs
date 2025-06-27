using Microsoft.EntityFrameworkCore;
using SpecialistService.API.Controllers;
using SpecialistService.API.DTOs;
using SpecialistService.API.Models;
using SpecialistService.API.Repositories;
using SpecialistService.API.Services.Interfaces;
using System.Security.Claims;

namespace SpecialistService.API.Services
{
    public class SkillService : ISkillService
    {
        private readonly SpecialistDbContext _context;
        private readonly ILogger<SpecialistSkillsController> _logger;
        private readonly IHttpContextAccessor _accessor;

        public SkillService(SpecialistDbContext context, ILogger<SpecialistSkillsController> logger, IHttpContextAccessor accessor)
        {
            _context = context;
            _logger = logger;
            _accessor = accessor;
        }

        public async Task<List<GetSkillsJson>?> GetSkillsAsync()
        {
            var user_id = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user_id == null)
            {
                _logger.LogWarning("Specialist not found: user_id is null");
                return null;
            }

            if (!int.TryParse(user_id, out int userId))
            {
                _logger.LogWarning("Failed to convert user_id to int: {UserId}", user_id);
                return null;
            }

            var specialist = await _context.Specialists
                .Include(s => s.Skills)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (specialist == null || specialist.Skills == null || specialist.Skills.Count == 0)
            {
                _logger.LogWarning("No skills found for specialist with ID: {SpecialistId}", specialist.Id);
                return null;
            }

            var skillsList = specialist.Skills.Select(skill => new GetSkillsJson
            {
                Id = skill.Id,
                Skill = skill.SkillName,
                SpecialistId = specialist.Id
            }).ToList();

            _logger.LogDebug("Successfully retrieved skills for specialist: {SpecialistId}", specialist.Id);
            return skillsList;
        }

        public async Task<string?> AddNewSkillAsync(AddSkillJson json)
        {
            var user_id = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user_id == null)
            {
                _logger.LogWarning("Specialist not found: user_id is null");
                return null;
            }

            if (!int.TryParse(user_id, out int userId))
            {
                _logger.LogWarning("Failed to convert user_id to int: {UserId}", user_id);
                return null;
            }

            var specialist = await _context.Specialists
                .Include(s => s.Skills)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (specialist == null || json.SkillName == null || specialist.Skills != null && specialist.Skills.Any(s => s.SkillName.Equals(json.SkillName, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Specialist not found with user_id: {UserId} || Skill name is empty for user_id || Skill already exists for user_id, SkillName: {SkillName}", userId, json.SkillName);
                return null;
            }

            var newSkill = new Skills
            {
                SkillName = json.SkillName,
                SpecialistsId = specialist.Id
            };

            specialist.Skills.Add(newSkill);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Skill successfully added for specialist: {SpecialistId}, SkillName: {SkillName}", specialist.Id, json.SkillName);
            return "Skill successfully added";
        } 

        public async Task<string?> DeleteExistSkillsAsync(DeleteSkillJson json)
        {
            var user_id = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user_id == null)
            {
                _logger.LogWarning("Specialist not found: user_id is null");
                return null;
            }

            if (!int.TryParse(user_id, out int userId))
            {
                _logger.LogWarning("Failed to convert user_id to int: {UserId}", user_id);
                return null;
            }

            var skill = await _context.Skills.FirstOrDefaultAsync(i => i.Id == json.SkillId);

            if (skill == null)
            {
                _logger.LogWarning("Skill not found with user_id: {UserId}", userId);
                return null;
            }

            _context.Skills.Remove(skill);

            await _context.SaveChangesAsync();


            _logger.LogDebug("Skill successfully remove {SkillName}", skill.SkillName);
            return "Skill successfully deleted";
        }
    }
}
