using HospitalManagementSystem.Dtos.Department;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Repository.Departments
{
    public interface IDepartmentsRepository
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<int> CreateAsync(DepartmentDto dto);
        Task<Department?> GetByIdAsync(int departmentId);
        Task<bool> UpdateAsync(DepartmentDto dto);
        Task<bool> DeleteAsync(int departmentId);
    }
}
