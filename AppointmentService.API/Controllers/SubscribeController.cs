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

        public SubscribeController(SubscribeService subscribeService)
        {
            _subscribeService = subscribeService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access")]
        public async Task<IActionResult> AppointmentsUser()
        {
            GetAppointmentJson? result = await _subscribeService.GetAppointmentUser();

            if (result == null) return NotFound("Нет данных");

            return Ok(result);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access")]
        public async Task<IActionResult> SubscribeAppointment(SignUpJson json)
        {
            string? result = await _subscribeService.Subscribe(json);

            if (result == null) return NotFound("Данные нет");

            return Ok(result);
        }
    }
}
