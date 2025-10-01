namespace HospitalManagementSystem.Dtos
{
    public class RegisterDoctorDto
    {
        public string Name { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int DepartmentId { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
      
    }
}
