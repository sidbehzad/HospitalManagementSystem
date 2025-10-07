using HospitalManagementSystem.Dtos.User;
using HospitalManagementSystem.Dtos.UserDtos;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Repository.Users
{
    public interface IUsersRepository
    {
        Task<IEnumerable<User>> GetAdminsAndDoctorsAsync();
        Task<int> RegisterDoctorAsync(RegisterDoctorDto dto);
        Task<User?> GetByIdAsync(int userId);
        Task<bool> UpdateUserAsync(EditUserDto dto);
        Task<bool> DeleteUserAsync(int userId);

        // Doctor-specific
        Task<EditDoctorDto?> GetDoctorProfileAsync(int doctorId);
        Task<bool> UpdateDoctorProfileAsync(EditDoctorDto dto);

        // Patient-specific
        Task<EditPatientDto?> GetPatientProfileAsync(int patientId);
        Task<bool> UpdatePatientProfileAsync(EditPatientDto dto);
    }
}
