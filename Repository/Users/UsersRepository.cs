using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.User;
using HospitalManagementSystem.Dtos.UserDtos;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace HospitalManagementSystem.Repository.Users
{
    public class UsersRepository : IUsersRepository
    {
        private readonly DapperContext _context;

        public UsersRepository(DapperContext context)
        {
            _context = context;
        }

      
        // GET USERS
        

        
        public async Task<IEnumerable<User>> GetAdminsAndDoctorsAsync()
        {
            using var db = _context.CreateConnection();
            return await db.QueryAsync<User>(
                "SELECT UserId, Email, Role FROM Users WHERE Role IN ('Admin','Doctor')");
        }

        
        public async Task<User?> GetByIdAsync(int userId)
        {
            using var db = _context.CreateConnection();
            return await db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE UserId=@UserId", new { UserId = userId });
        }

     
        // REGISTER
        
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

        
        // UPDATE
        

        
        public async Task<bool> UpdateUserAsync(EditUserDto dto)
        {
            using var db = _context.CreateConnection();
            var sql = "UPDATE Users SET Email=@Email, Role=@Role WHERE UserId=@UserId";
            var rows = await db.ExecuteAsync(sql, dto);
            return rows > 0;
        }

        
        public async Task<EditDoctorDto?> GetDoctorProfileAsync(int doctorId)
        {
            using var db = _context.CreateConnection();
            var sql = @"SELECT d.DoctorId, u.Email, d.Name, d.Specialization AS Specialization, d.DepartmentId
                        FROM Doctors d
                        JOIN Users u ON d.UserId = u.UserId
                        WHERE d.DoctorId = @DoctorId";
            return await db.QueryFirstOrDefaultAsync<EditDoctorDto>(sql, new { DoctorId = doctorId });
        }

        public async Task<bool> UpdateDoctorProfileAsync(EditDoctorDto dto)
        {
            using var db = _context.CreateConnection();
            var sql = @"UPDATE Doctors SET Name=@Name, Specialization=@Specialization, DepartmentId=@DepartmentId
                        WHERE DoctorId=@DoctorId;
                        UPDATE Users SET Email=@Email WHERE UserId=(SELECT UserId FROM Doctors WHERE DoctorId=@DoctorId)";
            var rows = await db.ExecuteAsync(sql, dto);
            return rows > 0;
        }

        
        public async Task<EditPatientDto?> GetPatientProfileAsync(int patientId)
        {
            using var db = _context.CreateConnection();
            var sql = @"SELECT p.PatientId, u.Email, p.Name, p.Age, p.Gender
                        FROM Patients p
                        JOIN Users u ON p.UserId = u.UserId
                        WHERE p.PatientId=@PatientId";
            return await db.QueryFirstOrDefaultAsync<EditPatientDto>(sql, new { PatientId = patientId });
        }

        public async Task<bool> UpdatePatientProfileAsync(EditPatientDto dto)
        {
            using var db = _context.CreateConnection();
            var sql = @"UPDATE Patients SET Name=@Name, Age=@Age, Gender=@Gender WHERE PatientId=@PatientId;
                        UPDATE Users SET Email=@Email WHERE UserId=(SELECT UserId FROM Patients WHERE PatientId=@PatientId)";
            var rows = await db.ExecuteAsync(sql, dto);
            return rows > 0;
        }

       
        // DELETE
        

        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var db = _context.CreateConnection();
            db.Open();
            using var transaction = db.BeginTransaction();

            try
            {
                // Get role
                var role = await db.QueryFirstOrDefaultAsync<string>(
                    "SELECT Role FROM Users WHERE UserId=@UserId", new { UserId = userId }, transaction);

                if (role == null) return false;

                if (role == "Doctor")
                {
                    // 1. Delete appointments of this doctor
                    await db.ExecuteAsync(
                        "DELETE FROM Appointments WHERE DoctorId=(SELECT DoctorId FROM Doctors WHERE UserId=@UserId)",
                        new { UserId = userId }, transaction);

                    // 2. Delete medical records of this doctor
                    await db.ExecuteAsync(
                        "DELETE FROM MedicalRecords WHERE DoctorId=(SELECT DoctorId FROM Doctors WHERE UserId=@UserId)",
                        new { UserId = userId }, transaction);

                    // 3. Delete the doctor
                    await db.ExecuteAsync(
                        "DELETE FROM Doctors WHERE UserId=@UserId", new { UserId = userId }, transaction);
                }
                else if (role == "Patient")
                {
                    await db.ExecuteAsync(
                        "DELETE FROM Patients WHERE UserId=@UserId", new { UserId = userId }, transaction);
                }

                // 4. Delete the user
                await db.ExecuteAsync(
                    "DELETE FROM Users WHERE UserId=@UserId", new { UserId = userId }, transaction);

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
