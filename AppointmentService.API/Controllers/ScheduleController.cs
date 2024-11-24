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
    public class ScheduleController : ControllerBase
    {
        private readonly AppointmentDbContext _context;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(AppointmentDbContext context, ILogger<ScheduleController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> GetScheduleSpecialist()
        {
            try
            {
                _logger.LogInformation("Attempting to get schedule for specialist");

                var userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    _logger.LogWarning("Failed to retrieve user ID from claims");
                    return NotFound("Specialist not found");
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Failed to convert user_id to int: {UserId}", userIdClaim);
                    return BadRequest("Invalid user identifier");
                }

                var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.UserId == userId);

                if (schedule == null || schedule.DateTime == null || !schedule.DateTime.Any())
                {
                    _logger.LogWarning("No schedule found for userId: {UserId}", userId);
                    return Ok(new List<GetScheduleJson>());
                }

                return Ok(new GetScheduleJson
                {
                    Id = schedule.SpecialistId,
                    dateTimes = schedule.DateTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting schedule for userId: {UserId}",
                                 HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error occurred while getting schedule");
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Access")]
        public async Task<IActionResult> GetScheduleSpecialistForId(int specialistId)
        {
            try
            {
                _logger.LogInformation("Attempting to get schedule for specialist with ID: {SpecialistId}", specialistId);

                if (specialistId == null || specialistId <= 0)
                {
                    _logger.LogWarning("Invalid specialistId provided: {SpecialistId}", specialistId);
                    return BadRequest("Invalid specialist ID");
                }

                var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.SpecialistId == specialistId);

                if (schedule == null || schedule.DateTime == null || !schedule.DateTime.Any())
                {
                    _logger.LogWarning("No schedule found for specialistId: {SpecialistId}", specialistId);
                    return Ok(new List<GetScheduleJson>());
                }

                return Ok(new GetScheduleJson
                {
                    Id = schedule.SpecialistId,
                    dateTimes = schedule.DateTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting schedule for specialistId: {SpecialistId}", specialistId);
                return StatusCode(500, "Internal server error occurred while getting schedule");
            }
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> AddScheduleSpecialist(AddScheduleJson json)
        {
            try
            {
                _logger.LogInformation("Attempting to add schedule for specialist");

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

                var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.UserId == userId);

                if (schedule == null)
                {
                    _logger.LogWarning("Schedule not found for userId: {UserId}", userId);
                    return NotFound("Schedule not found");
                }

                _logger.LogInformation("Found schedule for userId: {UserId}", userId);

                schedule.DateTime = json.dateTimes;
                _context.Schedules.Update(schedule);

                _logger.LogInformation("Schedule updated for userId: {UserId}", userId);

                foreach (var date in json.dateTimes)
                {
                    var appointments = new Appointments
                    {
                        CreatedDate = DateTime.UtcNow,
                        SpecilistsId = schedule.SpecialistId,
                        AppointmentDate = date,
                        SchedulesId = schedule.Id
                    };
                    await _context.Appointments.AddAsync(appointments);

                    _logger.LogInformation("Appointment added for SpecialistId: {SpecialistId}, AppointmentDate: {AppointmentDate}", schedule.SpecialistId, date);

                }
                await _context.SaveChangesAsync();

                return Ok("Successfully added schedule specialist");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding schedule for userId: {UserId}", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error occurred while adding schedule");
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = "Access", Roles = "Specialist")]
        public async Task<IActionResult> UpdateScheduleSpecialist(UpdateScheduleJson json)
        {
            try
            {
                _logger.LogInformation("Attempting to update schedule for specialist");

                var userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    _logger.LogWarning("Failed to retrieve user ID from claims");
                    return NotFound("Specialist not found");
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Failed to convert userId to int: {UserId}", userIdClaim);
                    return BadRequest("Invalid user ID");
                }

                var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.UserId == userId);

                if (schedule == null)
                {
                    _logger.LogWarning("Schedule not found for userId: {UserId}", userId);
                    return NotFound("Schedule not found");
                }

                var conflictingAppointments = await _context.Appointments
                    .Where(a => a.SchedulesId == schedule.Id && a.UserId != null)
                    .ToListAsync();

                if (conflictingAppointments.Any())
                {
                    _logger.LogWarning("Cannot update schedule for userId: {UserId} as users are already booked", userId);
                    return BadRequest("Cannot update schedule as users are already booked for some appointments.");
                }

                var existingAppointments = await _context.Appointments
                    .Where(a => a.SchedulesId == schedule.Id)
                    .ToListAsync();

                var removedDates = existingAppointments
                    .Where(a => !json.NewDateTimes.Contains((DateTime)a.AppointmentDate))
                    .ToList();

                _context.Appointments.RemoveRange(removedDates);

                var existingDates = existingAppointments.Select(a => a.AppointmentDate).ToHashSet();
                var addedDates = json.NewDateTimes
                    .Where(date => !existingDates.Contains(date))
                    .ToList();

                foreach (var date in addedDates)
                {
                    var newAppointment = new Appointments
                    {
                        CreatedDate = DateTime.UtcNow,
                        SpecilistsId = schedule.SpecialistId,
                        AppointmentDate = date,
                        SchedulesId = schedule.Id
                    };

                    _context.Appointments.Add(newAppointment);
                }

                schedule.DateTime = json.NewDateTimes;
                _context.Schedules.Update(schedule);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Schedule updated successfully for userId: {UserId}", userId);
                return Ok("Schedule updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating schedule for userId: {UserId}", HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, "Internal server error occurred while updating schedule");
            }
        }
    }
}
