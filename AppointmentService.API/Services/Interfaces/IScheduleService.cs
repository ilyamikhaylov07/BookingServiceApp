using AppointmentService.API.DTOs;

namespace AppointmentService.API.Services.Interfaces
{
    public interface IScheduleService
    {
        public Task<GetScheduleJson?> GetScheduleSpecialistAsync();
        public Task<GetScheduleJson?> GetSpecialistForIdAsync(int specialistId);
        public Task<string?> AddNewScheduleAsync(AddScheduleJson json);
        public Task<string?> UpdateScheduleAsync(UpdateScheduleJson json);
    }
}
