using AppointmentService.API.DTOs;
using AppointmentService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentService.API.Controllers
{
    [ApiController]
    [Route("AppointmentService/[controller]/[action]")]
    public class ScheduleController : ControllerBase
    {
        private readonly ILogger<ScheduleController> _logger;
        private readonly ScheduleService _scheduleService;

        public ScheduleController(ILogger<ScheduleController> logger, ScheduleService scheduleService)
        {
            _logger = logger;
            _scheduleService = scheduleService;
        }

        /// <summary>
        /// Получить расписание текущего специалиста
        /// </summary>
        /// <response code="200">Расписание успешно получено</response>
        /// <response code="404">Расписание не найдено</response>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        [ProducesResponseType(typeof(GetScheduleJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ScheduleSpecialist()
        {
            GetScheduleJson? result = await _scheduleService.GetScheduleSpecialist();

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null)
            {
                return NotFound("Не найдено");
            }

            return Ok(result);
        }

        /// <summary>
        /// Получить расписание специалиста по ID
        /// </summary>
        /// <param name="specialistId">ID специалиста</param>
        /// <response code="200">Расписание успешно получено</response>
        /// <response code="404">Данные не найдены</response>
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access")]
        [ProducesResponseType(typeof(GetScheduleJson), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ScheduleSpecialistForId(int specialistId)
        {
            GetScheduleJson? result = await _scheduleService.GetSpecialistForId(specialistId);

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        /// <summary>
        /// Добавить новое расписание для специалиста
        /// </summary>
        /// <param name="json">Данные расписания для добавления</param>
        /// <response code="200">Расписание успешно добавлено</response>
        /// <response code="404">Ошибка добавления, данные не найдены</response>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddScheduleSpecialist(AddScheduleJson json)
        {
            string? result = await _scheduleService.AddNewSchedule(json);

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok("Successfully added schedule specialist");
        }

        /// <summary>
        /// Обновить расписание специалиста
        /// </summary>
        /// <param name="json">Данные для обновления расписания</param>
        /// <response code="200">Расписание успешно обновлено</response>
        /// <response code="404">Ошибка обновления, данные не найдены</response>
        [HttpPut]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateScheduleSpecialist(UpdateScheduleJson json)
        {
            string? result = await _scheduleService.UpdateSchedule(json);

            if (result == null) return NotFound("Нет данных");

            _logger.LogInformation("Ответ успешно отправлен");

            return Ok(result);
        }
    }
}
