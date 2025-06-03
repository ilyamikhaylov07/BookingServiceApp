using AppointmentService.API.DTOs;

namespace AppointmentService.API.Services.Interfaces
{
    public interface IScheduleService
    {
        public Task<GetScheduleJson?> GetScheduleSpecialist();
        public Task<GetScheduleJson?> GetSpecialistForId(int specialistId);
        public Task<string?> AddNewSchedule(AddScheduleJson json);
        public Task<string?> UpdateSchedule(UpdateScheduleJson json);
    }
}
