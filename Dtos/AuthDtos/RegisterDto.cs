namespace HospitalManagementSystem.Dtos.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public string Gender { get; set; } = null!;
        public string? Contact { get; set; }
        public string? Address { get; set; }
    }
}
