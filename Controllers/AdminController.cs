using HospitalManagementSystem.Dtos.User;
using HospitalManagementSystem.Repository.Users;
using HospitalManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace HospitalManagementSystem.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly IUsersRepository _usersRepo;
        private readonly DapperContext _context;

        public AdminDashboardController(IUsersRepository usersRepo, DapperContext context)
        {
            _usersRepo = usersRepo;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            using var conn = _context.CreateConnection();

            // ✅ Fetch counts
            var doctorCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Doctors");
            var patientCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Patients");

            ViewBag.DoctorCount = doctorCount;
            ViewBag.PatientCount = patientCount;

            // Get list of admins and doctors
            var users = await _usersRepo.GetAdminsAndDoctorsAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult RegisterDoctor() => View(new RegisterDoctorDto());

        [HttpPost]
        public async Task<IActionResult> RegisterDoctor(RegisterDoctorDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            await _usersRepo.RegisterDoctorAsync(dto);
            return RedirectToAction("Index");
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

            var updated = await _usersRepo.UpdateUserAsync(dto);
            if (!updated)
            {
                ModelState.AddModelError("", "Error updating user.");
                return View(dto);
            }

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
