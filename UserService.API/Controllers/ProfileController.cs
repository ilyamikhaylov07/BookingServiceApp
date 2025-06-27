using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.API.DTOs;
using UserService.API.Services;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("UserService/[controller]/[action]")]
    public class ProfileController : ControllerBase
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly ProfileService _profileService;

        public ProfileController(ILogger<ProfileController> logger, ProfileService profileService)
        {
            _profileService = profileService;
            _logger = logger;
        }

        /// <summary>
        /// Получить профиль текущего пользователя
        /// </summary>
        /// <response code="200">Профиль успешно получен</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Профиль не найден</response>
        [Authorize(AuthenticationSchemes = "Access")]
        [HttpGet]
        [ProducesResponseType(typeof(GetProfileJson), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Profile()
        {
            var result = await _profileService.GetProfileAsync();

            _logger.LogInformation("Ответ успешно отправлен");

            return result;
        }

        /// <summary>
        /// Добавить данные в профиль пользователя
        /// </summary>
        /// <param name="json">Данные для добавления в профиль</param>
        /// <response code="200">Данные успешно добавлены</response>
        /// <response code="400">Некорректные данные</response>
        /// <response code="401">Пользователь не авторизован</response>
        [Authorize(AuthenticationSchemes = "Access")]
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddDataProfile(AddDataProfileJson json)
        {
            var result = await _profileService.AddDataProfileAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");

            return result;
        }

        /// <summary>
        /// Обновить профиль пользователя
        /// </summary>
        /// <param name="json">Новые данные профиля</param>
        /// <response code="200">Профиль успешно обновлен</response>
        /// <response code="400">Некорректные данные</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Профиль не найден</response>
        [Authorize(AuthenticationSchemes = "Access")]
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(AddDataProfileJson json)
        {
            var result = await _profileService.UpdateProfileAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");

            return result;
        }

        /// <summary>
        /// Очистить профиль пользователя
        /// </summary>
        /// <response code="200">Профиль успешно очищен</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="404">Профиль не найден</response>
        [Authorize(AuthenticationSchemes = "Access")]
        [HttpPatch]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClearProfile()
        {
            var result = await _profileService.ClearProfileAsync();

            _logger.LogInformation("Ответ успешно отправлен");

            return result;
        }
    }
}