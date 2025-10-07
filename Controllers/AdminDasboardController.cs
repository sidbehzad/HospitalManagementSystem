using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.User;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Repository.Departments;
using HospitalManagementSystem.Repository.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {

        private readonly IUsersRepository _usersRepo;
        private readonly IDepartmentsRepository _departmentsRepo;
        private readonly DapperContext _context;

        public AdminDashboardController(DapperContext context,IUsersRepository user, IDepartmentsRepository departmentsRepo)
        {
            _context = context;
            _usersRepo = user;
            _departmentsRepo = departmentsRepo;
        }

        public async Task<IActionResult> Index()
        {
            using var db = _context.CreateConnection();

            // Get list of Admins & Doctors
            var users = await db.QueryAsync<User>(
                "SELECT UserId, Email, Role FROM Users WHERE Role IN ('Admin', 'Doctor')"
            );

            // Count Doctors
            var doctorCount = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Users WHERE Role = 'Doctor'"
            );

            // Count Patients
            var patientCount = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Users WHERE Role = 'Patient'"
            );

            ViewBag.DoctorCount = doctorCount;
            ViewBag.PatientCount = patientCount;

            return View(users);
        }


        [HttpGet]
        public async Task<IActionResult> RegisterDoctor()
        {
            var departments = await _departmentsRepo.GetAllAsync();
            ViewBag.Departments = departments
                .Select(d => new { d.DepartmentId, d.Name })
                .ToList();

            return View(new RegisterDoctorDto());
        }

        [HttpPost]
        public async Task<IActionResult> RegisterDoctor(RegisterDoctorDto dto)
        {
            if (!ModelState.IsValid)
            {
                var departments = await _departmentsRepo.GetAllAsync();
                ViewBag.Departments = departments
                    .Select(d => new { d.DepartmentId, d.Name })
                    .ToList();
                return View(dto);
            }

            try
            {
                await _usersRepo.RegisterDoctorAsync(dto);
                TempData["Success"] = "Doctor registered successfully!";
                return RedirectToAction("Index"); 
            }
            catch (Exception ex)
            {
                
                if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("duplicate"))
                {
                    if (ex.Message.Contains("Email"))
                        ModelState.AddModelError("Email", "This email is already registered.");
                    if (ex.Message.Contains("Contact"))
                        ModelState.AddModelError("Contact", "This phone number is already registered.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                }

                var departments = await _departmentsRepo.GetAllAsync();
                ViewBag.Departments = departments
                    .Select(d => new { d.DepartmentId, d.Name })
                    .ToList();

                return View(dto);
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _usersRepo.GetByIdAsync(id);
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
            await _usersRepo.UpdateUserAsync(dto);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _usersRepo.DeleteUserAsync(id);
            return RedirectToAction("Index");
        }
    }
}
