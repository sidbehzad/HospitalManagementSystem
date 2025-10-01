namespace HospitalManagementSystem.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int DepartmentId { get; set; }
    }
}
