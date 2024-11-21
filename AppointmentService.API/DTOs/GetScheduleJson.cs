namespace AppointmentService.API.DTOs
{
    public class GetScheduleJson
    {
        public int Id { get; set; }
        public required List<DateTime> dateTimes { get; set; }
    }
}
