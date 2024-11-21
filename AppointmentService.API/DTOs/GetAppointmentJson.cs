namespace AppointmentService.API.DTOs
{
    public class GetAppointmentJson
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SpecialistId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
