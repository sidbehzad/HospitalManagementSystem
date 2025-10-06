using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.User;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace HospitalManagementSystem.Repository.Users
{
    public class UsersRepository: IUsersRepository
    {
        private readonly DapperContext _context;

        public UsersRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAdminsAndDoctorsAsync()
        {
            using var db = _context.CreateConnection();
            var sql = "SELECT UserId, Email, Role FROM Users WHERE Role IN ('Admin','Doctor')";
            return await db.QueryAsync<User>(sql);
        }

        public async Task<int> RegisterDoctorAsync(RegisterDoctorDto dto)
        {
            using var db = _context.CreateConnection();
            var hashedPassword = new PasswordHasher<User>().HashPassword(null, dto.Password);

            var parameters = new DynamicParameters();
            parameters.Add("@Email", dto.Email);
            parameters.Add("@PasswordHash", hashedPassword);
            parameters.Add("@Name", dto.Name);
            parameters.Add("@DepartmentId", dto.DepartmentId);
            parameters.Add("@Specialisation", dto.Specialization);
            parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await db.ExecuteAsync("sp_RegisterDoctor", parameters, commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@UserId");
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE UserId=@UserId",
                new { UserId = userId }
            );
        }

        public async Task<bool> UpdateUserAsync(EditUserDto dto)
        {
            using var db = _context.CreateConnection();
            var rows = await db.ExecuteAsync(
                "UPDATE Users SET Email=@Email, Role=@Role WHERE UserId=@UserId",
                dto
            );
            return rows > 0;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var db = _context.CreateConnection();
            using var transaction = db.BeginTransaction();

            try
            {
                var role = await db.QueryFirstOrDefaultAsync<string>(
                    "SELECT Role FROM Users WHERE UserId=@UserId",
                    new { UserId = userId },
                    transaction
                );

                if (role == null) return false;

                if (role == "Doctor")
                {
                    await db.ExecuteAsync(
                        "DELETE FROM DoctorPatients WHERE DoctorId IN (SELECT DoctorId FROM Doctors WHERE UserId=@UserId)",
                        new { UserId = userId },
                        transaction
                    );
                }
                else if (role == "Patient")
                {
                    await db.ExecuteAsync(
                        "DELETE FROM DoctorPatients WHERE PatientId IN (SELECT PatientId FROM Patients WHERE UserId=@UserId)",
                        new { UserId = userId },
                        transaction
                    );
                }

                await db.ExecuteAsync(
                    "DELETE FROM Users WHERE UserId=@UserId",
                    new { UserId = userId },
                    transaction
                );

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
