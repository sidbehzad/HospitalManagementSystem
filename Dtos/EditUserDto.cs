namespace HospitalManagementSystem.Dtos
{
    public class EditUserDto
    {

        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!; // Admin / Doctor / Patient
    }
}
