using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos;
using HospitalManagementSystem.Dtos.Department;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly DapperContext _context;
        public DepartmentController(DapperContext dapper)
        {

            _context = dapper;
        }


        public async Task<IActionResult> Index()
        {
            using var conn = _context.CreateConnection();
            var department = (await conn.QueryAsync<User>("SELECT * FROM Department")).ToList();
            return View(department);
        }
        public async Task<IActionResult> Create()
        {
           
            return View();
        }
        public async Task<IActionResult> Create(DepartmentDto dto)
        {
            using var conn = _context.CreateConnection();
           var departmentId= await conn.ExecuteScalarAsync<Department>("Insert into Department (Name) OUTPUT Inserted.id Values (@Name)", new { @Name = dto.Name });
            return RedirectToAction("Create");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            using var db = _context.CreateConnection();

            var department = await db.QueryFirstOrDefaultAsync<Department>(
                "SELECT * FROM Departments WHERE DepartmentId = @Id",
                new { Id = id }
            );

            if (department == null) return NotFound();

            var dto = new DepartmentDto
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Update(DepartmentDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            using var conn = _context.CreateConnection();

            var rowsAffected = await conn.ExecuteAsync(
                "UPDATE Departments SET Name = @Name WHERE DepartmentId = @DepartmentId",
                new { dto.Name, dto.DepartmentId }
            );

            if (rowsAffected == 0)
            {
                return NotFound();
            }

            return RedirectToAction("Index"); 
        }


        public async Task<IActionResult> DeleteDepartment(int departmentId)
        {
            using var db = _context.CreateConnection();

            using var transaction = db.BeginTransaction();

            try
            {
                // 1️⃣ Get all doctors in this department
                var doctorIds = (await db.QueryAsync<int>(
                    "SELECT DoctorId FROM Doctors WHERE DepartmentId = @DeptId",
                    new { DeptId = departmentId },
                    transaction
                )).ToList();

                // 2️⃣ Delete related entries from DoctorPatients
                if (doctorIds.Any())
                {
                    await db.ExecuteAsync(
                        "DELETE FROM DoctorPatients WHERE DoctorId IN @Ids",
                        new { Ids = doctorIds },
                        transaction
                    );
                }

                // 3️⃣ Delete doctors in this department
                await db.ExecuteAsync(
                    "DELETE FROM Doctors WHERE DepartmentId = @DeptId",
                    new { DeptId = departmentId },
                    transaction
                );

                // 4️⃣ Delete the department itself
                var rows = await db.ExecuteAsync(
                    "DELETE FROM Departments WHERE DepartmentId = @DeptId",
                    new { DeptId = departmentId },
                    transaction
                );

                if (rows == 0)
                {
                    transaction.Rollback();
                    return NotFound(); // department not found
                }

                // 5️⃣ Commit transaction
                transaction.Commit();

                return RedirectToAction("Index"); // back to department list
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                // log exception here if needed
                return StatusCode(500, "Error deleting department: " + ex.Message);
            }
        }


    }
}
