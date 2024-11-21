namespace AppointmentService.API.DTOs
{
    public class AddScheduleJson
    {
        public required List<DateTime> dateTimes { get; set; }
    }
}
