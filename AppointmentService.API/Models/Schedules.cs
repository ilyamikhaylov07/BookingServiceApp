using System.ComponentModel.DataAnnotations;

namespace AppointmentService.API.Models
{
    public class Schedules
    {
        [Key]
        public int Id { get; set; }
        public int SpecialistId { get; set; }
        public int UserId { get; set; }
        public List<DateTime>? DateTime { get; set; }
        public List<Appointments>? Appointments { get; set; }
    }
}
