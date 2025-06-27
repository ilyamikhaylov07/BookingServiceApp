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

        /// <summary>
        /// Получить список навыков специалиста
        /// </summary>
        /// <response code="200">Список навыков успешно получен</response>
        /// <response code="404">Навыки не найдены</response>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        [ProducesResponseType(typeof(List<GetSkillsJson>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Skills()
        {
            List<GetSkillsJson>? result = await _skillService.GetSkillsAsync();

            _logger.LogInformation("Ответ успешно отправлен");
            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        /// <summary>
        /// Добавить новый навык специалисту
        /// </summary>
        /// <param name="json">Данные нового навыка</param>
        /// <response code="200">Навык успешно добавлен</response>
        /// <response code="404">Ошибка при добавлении навыка</response>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddSkill(AddSkillJson json)
        {
            string? result = await _skillService.AddNewSkillAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");
            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        /// <summary>
        /// Удалить навыки специалиста
        /// </summary>
        /// <param name="json">Данные для удаления навыков</param>
        /// <response code="200">Навыки успешно удалены</response>
        /// <response code="404">Ошибка при удалении навыков</response>
        [HttpDelete]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSkills(DeleteSkillJson json)
        {
            var result = await _skillService.DeleteExistSkillsAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");
            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }
    }
}