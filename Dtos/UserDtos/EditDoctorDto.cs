namespace HospitalManagementSystem.Dtos.UserDtos
{
    public class EditDoctorDto
    {
        public int DoctorId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int DepartmentId { get; set; }
    }
}
