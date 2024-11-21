using AppointmentService.API.DTOs;
using AppointmentService.API.Models;
using AppointmentService.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppointmentService.API.Controllers
{
    [ApiController]
    [Route("AppointmentService/[controller]/[action]")]


    public class SignUpUserController : ControllerBase
    {
        private readonly AppointmentDbContext _context;
        private readonly ILogger<SignUpUserController> _logger;

        public SignUpUserController(AppointmentDbContext context, ILogger<SignUpUserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access")]
        public async Task<IActionResult> GetAppointmentsUser()
        {
            try
            {
                _logger.LogInformation("Attempting to get appointments for user");

                var user_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (user_id == null)
                {
                    _logger.LogWarning("Failed to retrieve user ID from claims");
                    return NotFound("Specialist not found");
                }

                if (!int.TryParse(user_id, out int userId))
                {
                    _logger.LogWarning("Failed to convert user_id to int: {UserId}", user_id);
                    return BadRequest("Неверный идентификатор пользователя");
                }

                var appointments = await _context.Appointments.FirstOrDefaultAsync(s => s.UserId == userId);

                if (appointments == null)
                {
                    _logger.LogWarning("Appointment not found for userId: {UserId}", userId);
                    return NotFound("Appointments not found");
                }

                return Ok(new GetAppointmentJson
                {
                    Id = appointments.Id,
                    UserId = (int)appointments.UserId,
                    SpecialistId = (int)appointments.SpecilistsId,
                    Status = appointments.Status,
                    AppointmentDate = (DateTime)appointments.AppointmentDate,
                    CreatedDate = (DateTime)appointments.CreatedDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while get appointments for userId: {UserId}", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error occurred while adding schedule");
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access")]
        public async Task<IActionResult> SignUp(SignUpJson json)
        {
            try
            {
                _logger.LogInformation("Attempting to sign up for user");

                var user_id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (user_id == null)
                {
                    _logger.LogWarning("Failed to retrieve user ID from claims");
                    return NotFound("Specialist not found");
                }

                if (!int.TryParse(user_id, out int userId))
                {
                    _logger.LogWarning("Failed to convert user_id to int: {UserId}", user_id);
                    return BadRequest("Неверный идентификатор пользователя");
                }

                var appointments = await _context.Appointments.FirstOrDefaultAsync(s => s.SpecilistsId == json.SpecialistId && s.AppointmentDate == json.DateTime);

                if (appointments == null)
                {
                    _logger.LogWarning("Appointments not found for userId: {UserId}", userId);
                    return NotFound("Appointments not found");
                }

                if(appointments.Status != null)
                {
                    _logger.LogInformation("Appointment close");
                    return BadRequest("Appointment close");
                }
                _logger.LogInformation("Found schedule for userId: {UserId}", userId);

                appointments.Status = "Занято";
                appointments.UserId = userId;
                _context.Appointments.Update(appointments);

                _logger.LogInformation("Schedule updated for userId: {UserId}", userId);

                await _context.SaveChangesAsync();

                return Ok("Successfully signup appointments for specialist");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while signUp appointments for userId: {UserId}", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error occurred while adding schedule");
            }
        }
    }
}
