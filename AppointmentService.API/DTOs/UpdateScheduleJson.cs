namespace AppointmentService.API.DTOs
{
    public class UpdateScheduleJson
    {
        public required List<DateTime> NewDateTimes { get; set; }
    }
}
