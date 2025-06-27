using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpecialistService.API.DTOs;
using SpecialistService.API.Services;

namespace SpecialistService.API.Controllers
{
    [ApiController]
    [Route("SpecialistService/[controller]/[action]")]
    public class SpecialistSkillsController : ControllerBase
    {
        private readonly ILogger<SpecialistSkillsController> _logger;
        private readonly SkillService _skillService;
        public SpecialistSkillsController(ILogger<SpecialistSkillsController> logger, SkillService skillService)
        {
            _skillService = skillService;
            _logger = logger;
        }
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> Skills()
        {
            List<GetSkillsJson>? result = await _skillService.GetSkillsAsync();

            _logger.LogInformation("Ответ успешно отправлен");
            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> AddSkill(AddSkillJson json)
        {
            string? result = await _skillService.AddNewSkillAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");
            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> DeleteSkills(DeleteSkillJson json)
        {
            var result = await _skillService.DeleteExistSkillsAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");
            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }
    }
}
