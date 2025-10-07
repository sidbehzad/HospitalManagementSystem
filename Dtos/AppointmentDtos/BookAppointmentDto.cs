using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Dtos.Appointment
{
    public class BookAppointmentDto
    {
        [Required(ErrorMessage = "Please select a doctor")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "Please select an appointment date")]

        public DateTime AppointmentDate { get; set; }
    }
}
