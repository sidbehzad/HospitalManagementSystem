using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace HospitalManagementSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly DapperContext _context;
        public UserController(DapperContext dapper)
        {

            _context = dapper;
        }
        public async Task<IActionResult> Index()
        {
            using var connection = _context.CreateConnection();
            var users = await connection.QueryAsync<User>("SELECT UserId, Username, Role FROM Users where Role='Admin' or 'Doctor'");
            return View(users);
        }

        public async Task<IActionResult> RegisterDoctor(RegisterDoctorDto dto)
        {

            if (!ModelState.IsValid)
                return View(dto);

            using var db = _context.CreateConnection();


            try
            {
                var hashedPassword = new PasswordHasher<User>().HashPassword(null, dto.Password);

                var parameters = new DynamicParameters();
                parameters.Add("@Email", dto.Email);
                parameters.Add("@PasswordHash", hashedPassword);
                parameters.Add("@Name", dto.Name);
                parameters.Add("@DepartmentId", dto.DepartmentId);
                parameters.Add("@Specialisation", dto.Specialization);
                parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);



               

                await db.ExecuteAsync(
                    "sp_RegisterDoctor",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var userId = parameters.Get<int>("@UserId");

                return RedirectToAction("RegisterDoctor");
            }
            catch (Exception ex)
            {

                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            using var db = _context.CreateConnection();
            var user = await db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE UserId = @Id",
                new { Id = id }
            );

            if (user == null) return NotFound();

            var dto = new EditUserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            using var db = _context.CreateConnection();
            await db.ExecuteAsync(
                "UPDATE Users SET Email = @Email, Role = @Role WHERE UserId = @UserId",
                dto
            );

            return RedirectToAction("Index", "Users");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            using var db = _context.CreateConnection();
            

            using var transaction = db.BeginTransaction();

            try
            {
                
                var role = await db.QueryFirstOrDefaultAsync<string>(
                    "SELECT Role FROM Users WHERE UserId = @UserId",
                    new { UserId = userId },
                    transaction
                );

                if (role == null)
                {
                    return NotFound("User not found");
                }

                
                if (role == "Doctor")
                {
                    await db.ExecuteAsync(
                        "DELETE FROM DoctorPatients WHERE DoctorId IN (SELECT DoctorId FROM Doctors WHERE UserId = @UserId)",
                        new { UserId = userId },
                        transaction
                    );
                }
                else if (role == "Patient")
                {
                    await db.ExecuteAsync(
                        "DELETE FROM DoctorPatients WHERE PatientId IN (SELECT PatientId FROM Patients WHERE UserId = @UserId)",
                        new { UserId = userId },
                        transaction
                    );
                }

                
                await db.ExecuteAsync(
                    "DELETE FROM Users WHERE UserId = @UserId",
                    new { UserId = userId },
                    transaction
                );

                transaction.Commit();
                return RedirectToAction("Index", "Users");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
               
                return BadRequest("Delete failed: " + ex.Message);
            }
        }


    }
}
