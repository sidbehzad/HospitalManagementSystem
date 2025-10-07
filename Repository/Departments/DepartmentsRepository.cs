using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.Department;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Repository.Departments
{
    public class DepartmentsRepository: IDepartmentsRepository
    {
        private readonly DapperContext _context;

        public DepartmentsRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            using var db = _context.CreateConnection();
            return await db.QueryAsync<Department>("SELECT * FROM Departments");
        }

        public async Task<int> CreateAsync(DepartmentDto dto)
        {
            using var db = _context.CreateConnection();
            var sql = "INSERT INTO Departments (Name) OUTPUT Inserted.DepartmentId VALUES (@Name)";
            return await db.ExecuteScalarAsync<int>(sql, new { dto.Name });
        }

        public async Task<Department?> GetByIdAsync(int departmentId)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<Department>(
                "SELECT * FROM Departments WHERE DepartmentId=@Id",
                new { Id = departmentId });
        }

        public async Task<bool> UpdateAsync(DepartmentDto dto)
        {
            using var db = _context.CreateConnection();
            var rows = await db.ExecuteAsync(
                "UPDATE Departments SET Name=@Name WHERE DepartmentId=@DepartmentId",
                new { dto.Name, dto.DepartmentId });
            return rows > 0;
        }



        public async Task<bool> DeleteAsync(int departmentId)
        {
            using var db = _context.CreateConnection();
            

            var rows = await db.ExecuteAsync(
                "DELETE FROM Departments WHERE DepartmentId = @DeptId",
                new { DeptId = departmentId }
            );

            return rows > 0;
        }





    }
}
