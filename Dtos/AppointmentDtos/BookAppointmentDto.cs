using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Dtos.Appointment
{
    public class BookAppointmentDto
    {
        [Required(ErrorMessage = "Please select a doctor")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "Please select an appointment date")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(BookAppointmentDto), nameof(ValidateDate))]
        public DateTime AppointmentDate { get; set; }




        public static ValidationResult? ValidateDate(DateTime date, ValidationContext context)
        {
            if (date.Date < DateTime.Now.Date)
                return new ValidationResult("Appointment date cannot be in the past.");
            return ValidationResult.Success;
        }
    }
}
