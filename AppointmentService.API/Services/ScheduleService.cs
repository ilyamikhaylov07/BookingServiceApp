using AppointmentService.API.DTOs;
using AppointmentService.API.Models;
using AppointmentService.API.Repositories;
using AppointmentService.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppointmentService.API.Services
{
    public class ScheduleService : IScheduleService
    {
        private AppointmentDbContext _dbContext;
        private ILogger<ScheduleService> _logger;
        private IHttpContextAccessor _httpContextAccessor;

        public ScheduleService(AppointmentDbContext context, ILogger<ScheduleService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Получить расписание текущего специалиста из базы данных
        /// </summary>
        /// <returns>
        /// Объект <see cref="GetScheduleJson"/>, содержащий расписание специалиста, или null, если не найдено
        /// </returns>
        public async Task<GetScheduleJson?> GetScheduleSpecialistAsync()
        {
            string? userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                _logger.LogWarning("Failed to retrieve user ID from claims");
                return null;
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("Failed to convert user_id to int: {UserId}", userIdClaim);
                return null;
            }

            var schedule = await _dbContext.Schedules.FirstOrDefaultAsync(s => s.UserId == userId);

            if (schedule == null || schedule.DateTime == null || !schedule.DateTime.Any())
            {
                _logger.LogWarning("No schedule found for userId: {UserId}", userId);
                return null;
            }

            return new GetScheduleJson
            {
                Id = schedule.SpecialistId,
                dateTimes = schedule.DateTime
            };
        }


        /// <summary>
        /// Получить расписание специалиста по его идентификатору
        /// </summary>
        /// <param name="specialistId">Идентификатор специалиста</param>
        /// <returns>
        /// Объект <see cref="GetScheduleJson"/>, содержащий расписание специалиста, или null, если не найдено
        /// </returns>
        public async Task<GetScheduleJson?> GetSpecialistForIdAsync(int specialistId)
        {
            if (specialistId <= 0)
            {
                _logger.LogWarning("Invalid specialistId provided: {SpecialistId}", specialistId);
                return null;
            }

            Schedules? schedule = await _dbContext.Schedules.FirstOrDefaultAsync(s => s.SpecialistId == specialistId);

            if (schedule == null || schedule.DateTime == null || !schedule.DateTime.Any())
            {
                _logger.LogWarning("No schedule found for specialistId: {SpecialistId}", specialistId);
                return null;
            }

            return new GetScheduleJson
            {
                Id = schedule.SpecialistId,
                dateTimes = schedule.DateTime
            };
        }


        /// <summary>
        /// Добавить новое расписание для текущего специалиста и создать связанные записи приёмов
        /// </summary>
        /// <param name="json">Данные нового расписания</param>
        /// <returns>
        /// Строка с сообщением об успехе или null, если операция не удалась
        /// </returns>
        public async Task<string?> AddNewScheduleAsync(AddScheduleJson json)
        {
            var user_id = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(user_id, out int userId) || user_id == null)
            {
                _logger.LogWarning("Failed to convert user_id to int: {UserId} or Failed to retrieve user ID from claims", user_id);
                return null;
            }

            var schedule = await _dbContext.Schedules.FirstOrDefaultAsync(s => s.UserId == userId);

            if (schedule == null)
            {
                _logger.LogWarning("Schedule not found for userId: {UserId}", userId);
                return null;
            }

            _logger.LogDebug("Found schedule for userId: {UserId}", userId);

            schedule.DateTime = json.dateTimes;
            _dbContext.Schedules.Update(schedule);

            _logger.LogDebug("Schedule updated for userId: {UserId}", userId);

            foreach (var date in json.dateTimes)
            {
                var appointments = new Appointments
                {
                    CreatedDate = DateTime.UtcNow,
                    SpecilistsId = schedule.SpecialistId,
                    AppointmentDate = date,
                    SchedulesId = schedule.Id
                };
                await _dbContext.Appointments.AddAsync(appointments);

                _logger.LogDebug("Appointment added for SpecialistId: {SpecialistId}, AppointmentDate: {AppointmentDate}", schedule.SpecialistId, date);

            }
            await _dbContext.SaveChangesAsync();

            return "Successfully added schedule specialist";
        }


        /// <summary>
        /// Обновить текущее расписание специалиста, удалив устаревшие и добавив новые даты
        /// </summary>
        /// <param name="json">Объект с новыми датами расписания</param>
        /// <returns>
        /// Строка с сообщением об успешном обновлении или null, если обновление невозможно
        /// </returns>
        public async Task<string?> UpdateScheduleAsync(UpdateScheduleJson json)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId) || userIdClaim == null)
            {
                _logger.LogWarning("Failed to convert userId to int: {UserId} || Failed to retrieve user ID from claims", userIdClaim);
                return null;
            }

            var schedule = await _dbContext.Schedules.FirstOrDefaultAsync(s => s.UserId == userId);

            if (schedule == null)
            {
                _logger.LogWarning("Schedule not found for userId: {UserId}", userId);
                return null;
            }

            List<Appointments>? conflictingAppointments = await _dbContext.Appointments
                .Where(a => a.SchedulesId == schedule.Id && a.UserId != null)
                .ToListAsync();

            if (conflictingAppointments.Count == 0)
            {
                _logger.LogWarning("Cannot update schedule for userId: {UserId} as users are already booked", userId);
                return null;
            }

            List<Appointments>? existingAppointments = await _dbContext.Appointments
                .Where(a => a.SchedulesId == schedule.Id)
                .ToListAsync();

            List<Appointments>? removedDates = existingAppointments
                .Where(a => !json.NewDateTimes.Contains((DateTime)a.AppointmentDate))
                .ToList();

            _dbContext.Appointments.RemoveRange(removedDates);

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

                _dbContext.Appointments.Add(newAppointment);
            }

            schedule.DateTime = json.NewDateTimes;
            _dbContext.Schedules.Update(schedule);

            await _dbContext.SaveChangesAsync();

            return "Schedule updated successfully";
        }
    }
}
