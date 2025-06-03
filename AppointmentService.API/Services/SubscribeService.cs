using AppointmentService.API.Controllers;
using AppointmentService.API.DTOs;
using AppointmentService.API.Repositories;
using AppointmentService.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppointmentService.API.Services
{
    public class SubscribeService : ISubscribeService
    {
        private readonly AppointmentDbContext _context;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SubscribeController> _logger;

        public SubscribeService(AppointmentDbContext context, ILogger<SubscribeController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<GetAppointmentJson?> GetAppointmentUser()
        {
            var user_id = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(user_id, out int userId) || user_id == null)
            {
                _logger.LogWarning("Failed to convert user_id to int: {UserId} or Failed to retrieve user ID from claims", user_id);
                return null;
            }

            var appointments = await _context.Appointments.FirstOrDefaultAsync(s => s.UserId == userId);

            if (appointments == null)
            {
                _logger.LogWarning("Appointment not found for userId: {UserId}", userId);
                return null;
            }

            return new GetAppointmentJson
            {
                Id = appointments.Id,
                UserId = (int)appointments.UserId,
                SpecialistId = (int)appointments.SpecilistsId,
                Status = appointments.Status,
                AppointmentDate = (DateTime)appointments.AppointmentDate,
                CreatedDate = (DateTime)appointments.CreatedDate
            };
        }

        public async Task<string?> Subscribe(SignUpJson json)
        {
            var user_id = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(user_id, out int userId) || user_id == null)
            {
                _logger.LogWarning("Failed to convert user_id to int: {UserId} or Failed to retrieve user ID from claims", user_id);
                return null;
            }

            var appointments = await _context.Appointments.FirstOrDefaultAsync(s => s.SpecilistsId == json.SpecialistId && s.AppointmentDate == json.DateTime);

            if ( appointments == null || appointments.Status != null)
            {
                _logger.LogDebug("Appointment close");
                return null;
            }
            _logger.LogDebug("Found schedule for userId: {UserId}", userId);

            appointments.Status = "Занято";
            appointments.UserId = userId;
            _context.Appointments.Update(appointments);

            _logger.LogDebug("Schedule updated for userId: {UserId}", userId);

            await _context.SaveChangesAsync();

            return "Successfully signup appointments for specialist";
        }
    }
}
