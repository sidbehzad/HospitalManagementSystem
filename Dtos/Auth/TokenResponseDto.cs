namespace HospitalManagementSystem.Dtos.Auth
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = null!;   // New JWT token
        public string RefreshToken { get; set; } = null!;
    }
}
