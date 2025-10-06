using HospitalManagementSystem.Dtos.Auth;

namespace HospitalManagementSystem.Repository.Auths
{
    public interface IAuthsRepository
    {
      
            Task RegisterPatientAsync(RegisterDto dto);
            Task<TokenResponseDto?> LoginAsync(LoginDto dto);
            Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto dto);
     
    }
}
