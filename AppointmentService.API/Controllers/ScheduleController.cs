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

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
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

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access")]
        public async Task<IActionResult> ScheduleSpecialistForId(int specialistId)
        {
            GetScheduleJson? result = await _scheduleService.GetSpecialistForId(specialistId);

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> AddScheduleSpecialist(AddScheduleJson json)
        {
            string? result = await _scheduleService.AddNewSchedule(json);

            _logger.LogInformation("Ответ успешно отправлен");

            if (result == null) return NotFound("Нет данных");

            return Ok("Successfully added schedule specialist");
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> UpdateScheduleSpecialist(UpdateScheduleJson json)
        {
            string? result = await _scheduleService.UpdateSchedule(json);

            if (result == null) return NotFound("Нет данных");

            _logger.LogInformation("Ответ успешно отправлен");

            return Ok(result);
        }
    }
}
