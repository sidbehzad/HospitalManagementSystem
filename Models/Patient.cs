namespace HospitalManagementSystem.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public string Gender { get; set; } = null!;
        public string? Contact { get; set; }
        public string? Address { get; set; }


    }
}
