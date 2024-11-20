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
    public class SpecialistSkillsController : ControllerBase
    {
        private readonly SpecialistDbContext _context;
        private readonly ILogger<SpecialistSkillsController> _logger;
        public SpecialistSkillsController(SpecialistDbContext context, ILogger<SpecialistSkillsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> GetSkills()
        {
            _logger.LogInformation("Starting to get skills specialist");

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

                if (specialist.Skills == null || !specialist.Skills.Any())
                {
                    _logger.LogInformation("No skills found for specialist with ID: {SpecialistId}", specialist.Id);
                    return Ok(new List<GetSkillsJson>());
                }

                var skillsList = specialist.Skills.Select(skill => new GetSkillsJson
                {
                    Id = skill.Id,
                    Skill = skill.SkillName,
                    SpecialistId = specialist.Id
                }).ToList();

                _logger.LogInformation("Successfully retrieved skills for specialist: {SpecialistId}", specialist.Id);
                return Ok(skillsList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving skills for specialist with user_id: {UserId}", HttpContext.User.FindFirst(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> AddSkill(AddSkillJson json)
        {
            _logger.LogInformation("Starting to add skill for specialist");

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
                    return BadRequest("Invalid user ID");
                }

                var specialist = await _context.Specialists
                    .Include(s => s.Skills)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (specialist == null)
                {
                    _logger.LogWarning("Specialist not found with user_id: {UserId}", userId);
                    return NotFound("Specialist not exist");
                }

                if (string.IsNullOrWhiteSpace(json.SkillName))
                {
                    _logger.LogWarning("Skill name is empty for user_id: {UserId}", userId);
                    return BadRequest("Skill name must not be empty");
                }

                if (specialist.Skills != null && specialist.Skills.Any(s => s.SkillName.Equals(json.SkillName, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning("Skill already exists for user_id: {UserId}, SkillName: {SkillName}", userId, json.SkillName);
                    return Conflict("Skill already exists");
                }

                var newSkill = new Skills
                {
                    SkillName = json.SkillName,
                    SpecialistsId = specialist.Id
                };

                specialist.Skills.Add(newSkill);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Skill successfully added for specialist: {SpecialistId}, SkillName: {SkillName}", specialist.Id, json.SkillName);
                return Ok("Skill successfully added");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while add skill for specialist with user_id: {UserId}", HttpContext.User.FindFirst(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> DeleteSkill(DeleteSkillJson json)
        {
            _logger.LogInformation("Starting to add skill for specialist");

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
                    return BadRequest("Invalid user ID");
                }

                var skill = await _context.Skills.FirstOrDefaultAsync(i => i.Id == json.SkillId);

                if (skill == null)
                {
                    _logger.LogWarning("Skill not found with user_id: {UserId}", userId);
                    return NotFound("Skill not exist");
                }

                _context.Skills.Remove(skill);

                await _context.SaveChangesAsync();


                _logger.LogInformation("Skill successfully remove {SkillName}", skill.SkillName);
                return Ok("Skill successfully deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while remove skill for specialist with user_id: {UserId}", HttpContext.User.FindFirst(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
