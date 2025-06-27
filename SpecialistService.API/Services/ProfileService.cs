using Microsoft.EntityFrameworkCore;
using SpecialistService.API.Controllers;
using SpecialistService.API.DTOs;
using SpecialistService.API.Models;
using SpecialistService.API.Repositories;
using SpecialistService.API.Services.Interfaces;
using System.Security.Claims;

namespace SpecialistService.API.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ILogger<SpecialistProfileController> _logger;
        private readonly SpecialistDbContext _context;
        private readonly IHttpContextAccessor _accessor;

        public ProfileService(ILogger<SpecialistProfileController> logger, SpecialistDbContext context, IHttpContextAccessor contextAccessor)
        {
            _accessor = contextAccessor;
            _context = context;
            _logger = logger;
        }

        public async Task<List<GetProfileJson>?> GetAllProfilesAsync()
        {
            var specialists = await _context.Specialists
                .Include(s => s.Skills)
                .ToListAsync();

            if (specialists.Count == 0)
            {
                _logger.LogWarning("No specialists found");
                return null;
            }

            var result = specialists.Select(s => new GetProfileJson
            {
                Id = s.Id,
                SkillName = s.Skills?.Select(skill => skill.SkillName).ToList(),
                Profession = s.Profession,
                Description = s.Description
            }).ToList();

            _logger.LogDebug("Successfully retrieved all specialists: Count = {Count}", result.Count);
            return result;
        }

        public async Task<GetProfileJson?> GetProfileSpecialistAsync()
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

            if (specialist == null)
            {
                _logger.LogWarning("Specialist not found with user_id: {UserId}", userId);
                return null;
            }

            var skillNames = specialist.Skills?.Select(s => s.SkillName).ToList();

            _logger.LogDebug("Profile for specialist retrieved successfully: {SpecialistId}", specialist.Id);

            return new GetProfileJson
            {
                Id = specialist.Id,
                Description = specialist?.Description,
                Profession = specialist?.Profession,
                SkillName = skillNames
            };
        }

        public async Task<string?> AddNewDataAsync(ProfileJson json)
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

            if (specialist == null)
            {
                _logger.LogWarning("Specialist not found with user_id: {UserId}", userId);
                return null;
            }

            specialist.Profession = json.Profession ?? specialist.Profession;
            specialist.Description = json?.Description ?? specialist.Description;
            specialist.Skills = specialist.Skills ?? new List<Skills>();

            if (json?.SkillName != null && json.SkillName.Count != 0)
            {
                _logger.LogDebug("Clearing existing skills and adding new ones to specialist profile");
                specialist.Skills.Clear();
                foreach (var skillName in json.SkillName)
                {
                    specialist.Skills.Add(new Skills { SkillName = skillName });
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogDebug("Successfully added data to specialist profile for user_id: {UserId}", userId);

            return "Skill successfully added";
        }

        public async Task<string?> ChangeInfoProfileAsync(UpdateProfileJson json)
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

            if (specialist == null)
            {
                _logger.LogWarning("Specialist not found with user_id: {UserId}", userId);
                return null;
            }

            specialist.Profession = json?.Profession ?? specialist.Profession;
            specialist.Description = json?.Description ?? specialist.Description;
            specialist.Skills = specialist.Skills ?? new List<Skills>();
            if (json?.SkillName != null && json.SkillName.Count != 0)
            {
                _logger.LogDebug("Clearing existing skills and adding new ones to specialist profile");
                specialist.Skills.Clear();
                foreach (var skillName in json.SkillName)
                {
                    specialist.Skills.Add(new Skills { SkillName = skillName });
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogDebug("Successfully updated specialist profile for user_id: {UserId}", userId);

            return "Profile updated";
        }

        public async Task<string?> ClearProfileAsync()
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

            if (specialist == null)
            {
                _logger.LogWarning("Specialist not found with user_id: {UserId}", userId);
                return null;
            }

            specialist.Profession = null;
            specialist.Description = null;
            specialist.Skills = null;

            await _context.SaveChangesAsync();

            _logger.LogDebug("Successfully clear specialist profile for user_id: {UserId}", userId);

            return "Profile cleared";
        }
    }
}
