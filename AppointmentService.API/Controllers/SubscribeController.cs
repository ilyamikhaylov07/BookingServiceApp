using AppointmentService.API.DTOs;
using AppointmentService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentService.API.Controllers
{
    [ApiController]
    [Route("AppointmentService/[controller]/[action]")]


    public class SubscribeController : ControllerBase
    {
        private readonly SubscribeService _subscribeService;
        private readonly ILogger<SubscribeController> _logger;

        public SubscribeController(SubscribeService subscribeService, ILogger<SubscribeController> logger)
        {
            _subscribeService = subscribeService;
            _logger = logger;
        }

        /// <summary>
        /// Получить записи на приём текущего пользователя
        /// </summary>
        /// <response code="200">Данные успешно получены</response>
        /// <response code="404">Нет данных</response>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access")]
        [ProducesResponseType(typeof(GetAppointmentJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AppointmentsUser()
        {
            GetAppointmentJson? result = await _subscribeService.GetAppointmentUserAsync();

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        /// <summary>
        /// Записаться на приём
        /// </summary>
        /// <param name="json">Данные для записи на приём</param>
        /// <response code="200">Запись успешно создана</response>
        /// <response code="404">Нет данных</response>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubscribeAppointment(SignUpJson json)
        {
            string? result = await _subscribeService.SubscribeAsync(json);

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }
    }
}
