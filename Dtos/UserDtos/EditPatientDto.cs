namespace HospitalManagementSystem.Dtos.UserDtos
{
    public class EditPatientDto
    {
        public int PatientId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int Age { get; set; }
        public string Gender { get; set; } = null!;
    }
}
