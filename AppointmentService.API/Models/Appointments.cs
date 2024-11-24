using System.ComponentModel.DataAnnotations;

namespace AppointmentService.API.Models
{
    public class Appointments
    {
        [Key]
        public int Id {  get; set; } 
        public int? UserId { get; set; }
        public int? SpecilistsId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int SchedulesId { get; set; }
        public Schedules Schedules { get; set; }

    }
}
