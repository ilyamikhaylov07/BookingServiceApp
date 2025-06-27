using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpecialistService.API.DTOs;
using SpecialistService.API.Services;

namespace SpecialistService.API.Controllers
{
    [ApiController]
    [Route("SpecialistService/[controller]/[action]")]
    public class SpecialistProfileController : ControllerBase
    {
        private readonly ILogger<SpecialistProfileController> _logger;
        private readonly ProfileService _profileService;

        public SpecialistProfileController(ILogger<SpecialistProfileController> logger, ProfileService profileService)
        {
            _profileService = profileService;
            _logger = logger;
        }

        /// <summary>
        /// Получить все профили специалистов
        /// </summary>
        /// <response code="200">Данные успешно получены</response>
        /// <response code="404">Нет данных</response>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access")]
        [ProducesResponseType(typeof(List<GetProfileJson>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AllProfilesSpec()
        {
            List<GetProfileJson>? result = await _profileService.GetAllProfilesAsync();

            if (result == null) return NotFound("Нет данных");

            _logger.LogInformation("Ответ успешно отправлен");

            return Ok(result);
        }

        /// <summary>
        /// Получить профиль текущего специалиста
        /// </summary>
        /// <response code="200">Данные успешно получены</response>
        /// <response code="404">Нет данных</response>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        [ProducesResponseType(typeof(GetProfileJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProfileSpec()
        {
            GetProfileJson? result = await _profileService.GetProfileSpecialistAsync();

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        /// <summary>
        /// Добавить данные в профиль специалиста
        /// </summary>
        /// <param name="json">Данные для добавления в профиль</param>
        /// <response code="200">Данные успешно добавлены</response>
        /// <response code="404">Нет данных</response>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddDataProfileSpec(ProfileJson json)
        {
            string? result = await _profileService.AddNewDataAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        /// <summary>
        /// Обновить данные профиля специалиста
        /// </summary>
        /// <param name="json">Данные для обновления профиля</param>
        /// <response code="200">Данные успешно обновлены</response>
        [HttpPut]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDataProfileSpec(UpdateProfileJson json)
        {
            string? result = await _profileService.ChangeInfoProfileAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");

            return Ok(result);
        }

        /// <summary>
        /// Очистить профиль специалиста
        /// </summary>
        /// <response code="200">Профиль успешно очищен</response>
        /// <response code="404">Нет данных</response>
        [HttpPatch]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClearProfileSpec()
        {
            var result = await _profileService.ClearProfileAsync();

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }
    }
}