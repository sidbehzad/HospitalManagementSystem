namespace HospitalManagementSystem.Dtos.Appointment
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }     
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public string PatientName { get; set; } = null!;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = null!;



    }
}
