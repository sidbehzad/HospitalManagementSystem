namespace HospitalManagementSystem.Dtos.Auth
{
    public class RefreshTokenRequestDto
    {
        public int UserId { get; set; }          // Id of the user requesting a new access token
        public string RefreshToken { get; set; } = null!;
    }
}
