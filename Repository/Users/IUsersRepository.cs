using HospitalManagementSystem.Dtos.User;
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
    }
}
