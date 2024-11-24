using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecialistService.API.DTOs;
using SpecialistService.API.Models;
using SpecialistService.API.Repositories;
using System.Security.Claims;

namespace SpecialistService.API.Controllers
{
    [ApiController]
    [Route("SpecialistService/[controller]/[action]")]
    public class SpecialistProfileController : ControllerBase
    {
        private readonly ILogger<SpecialistProfileController> _logger;
        private readonly SpecialistDbContext _context;

        public SpecialistProfileController(ILogger<SpecialistProfileController> logger, SpecialistDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access")]
        public async Task<IActionResult> GetAllProfilesSpec()
        {
            _logger.LogInformation("Starting to get all profiles for specialists");

            try
            {
                var specialists = await _context.Specialists
                    .Include(s => s.Skills)
                    .ToListAsync();

                if (!specialists.Any())
                {
                    _logger.LogInformation("No specialists found");
                    return Ok(new List<GetProfileJson>());
                }
                
                var result = specialists.Select(s => new GetProfileJson
                {
                    Id = s.Id,
                    SkillName = s.Skills?.Select(skill => skill.SkillName).ToList(),
                    Profession = s.Profession,
                    Description = s.Description
                }).ToList();

                _logger.LogInformation("Successfully retrieved all specialists: Count = {Count}", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all specialists");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> GetProfileSpec()
        {
            _logger.LogInformation("Starting to retrieve profile for specialist");

            try
            {
                var user_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (user_id == null)
                {
                    _logger.LogWarning("Specialist not found: user_id is null");
                    return NotFound("Specialist not exist");
                }

                if (!int.TryParse(user_id, out int userId))
                {
                    _logger.LogWarning("Failed to convert user_id to int: {UserId}", user_id);
                    return BadRequest("Неверный идентификатор пользователя");
                }

                var specialist = await _context.Specialists
                    .Include(s => s.Skills)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (specialist == null)
                {
                    _logger.LogWarning("Specialist not found with user_id: {UserId}", userId);
                    return NotFound("Specialist not exist");
                }

                var skillNames = specialist.Skills?.Select(s => s.SkillName).ToList();

                _logger.LogInformation("Profile for specialist retrieved successfully: {SpecialistId}", specialist.Id);

                return Ok(new GetProfileJson
                {
                    Id = specialist.Id,
                    Description = specialist?.Description,
                    Profession = specialist?.Profession,
                    SkillName = skillNames
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving profile for specialist with user_id: {UserId}", HttpContext.User.FindFirst(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> AddDataProfileSpec(ProfileJson json)
        {
            _logger.LogInformation("Starting to add data to specialist profile");

            try
            {
                var user_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (user_id == null)
                {
                    _logger.LogWarning("Specialist not found: user_id is null");
                    return NotFound("Specialist not exist");
                }

                if (!int.TryParse(user_id, out int userId))
                {
                    _logger.LogWarning("Failed to convert user_id to int: {UserId}", user_id);
                    return BadRequest("Неверный идентификатор пользователя");
                }

                var specialist = await _context.Specialists
                    .Include(s => s.Skills)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (specialist == null)
                {
                    _logger.LogWarning("Specialist not found with user_id: {UserId}", userId);
                    return NotFound("Specialist not exist");
                }

                specialist.Profession = json.Profession ?? specialist.Profession;
                specialist.Description = json?.Description ?? specialist.Description;
                specialist.Skills = specialist.Skills ?? new List<Skills>();

                if (json.SkillName != null && json.SkillName.Any())
                {
                    _logger.LogInformation("Clearing existing skills and adding new ones to specialist profile");
                    specialist.Skills.Clear();
                    foreach (var skillName in json.SkillName)
                    {
                        specialist.Skills.Add(new Skills { SkillName = skillName });
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully added data to specialist profile for user_id: {UserId}", userId);

                return Ok("Skill successfully added");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding data to specialist profile for user_id: {UserId}", HttpContext.User.FindFirst(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> UpdateDataProfileSpec(UpdateProfileJson json)
        {
            _logger.LogInformation("Starting to update specialist profile");

            try
            {
                var user_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (user_id == null)
                {
                    _logger.LogWarning("Specialist not found: user_id is null");
                    return NotFound("Specialist not exist");
                }

                if (!int.TryParse(user_id, out int userId))
                {
                    _logger.LogWarning("Failed to convert user_id to int: {UserId}", user_id);
                    return BadRequest("Неверный идентификатор пользователя");
                }

                var specialist = await _context.Specialists
                    .Include(s => s.Skills)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (specialist == null)
                {
                    _logger.LogWarning("Specialist not found with user_id: {UserId}", userId);
                    return BadRequest("Specialist not exist");
                }

                specialist.Profession = json?.Profession ?? specialist.Profession;
                specialist.Description = json?.Description ?? specialist.Description;
                specialist.Skills = specialist.Skills ?? new List<Skills>();
                if (json?.SkillName != null && json.SkillName.Any())
                {
                    _logger.LogInformation("Clearing existing skills and adding new ones to specialist profile");
                    specialist.Skills.Clear();
                    foreach (var skillName in json.SkillName)
                    {
                        specialist.Skills.Add(new Skills { SkillName = skillName });
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated specialist profile for user_id: {UserId}", userId);

                return Ok("Profile updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating specialist profile for user_id: {UserId}", HttpContext.User.FindFirst(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPatch]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> ClearProfileSpec()
        {
            _logger.LogInformation("Starting to clear specialist profile");

            try
            {
                var user_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (user_id == null)
                {
                    _logger.LogWarning("Specialist not found: user_id is null");
                    return NotFound("Specialist not exist");
                }

                if (!int.TryParse(user_id, out int userId))
                {
                    _logger.LogWarning("Failed to convert user_id to int: {UserId}", user_id);
                    return BadRequest("Неверный идентификатор пользователя");
                }

                var specialist = await _context.Specialists
                    .Include(s => s.Skills)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (specialist == null)
                {
                    _logger.LogWarning("Specialist not found with user_id: {UserId}", userId);
                    return NotFound("Specialist not exist");
                }

                specialist.Profession = null;
                specialist.Description = null;
                specialist.Skills = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully clear specialist profile for user_id: {UserId}", userId);

                return Ok("Profile cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating specialist profile for user_id: {UserId}", HttpContext.User.FindFirst(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
