using HospitalManagementSystem.Repository.Users;
using HospitalManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    public class DoctorDashboardController : Controller
    {
        private readonly IUsersRepository _repo;
        private readonly DapperContext _context;

        public DoctorDashboardController(IUsersRepository repo, DapperContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int doctorId = int.Parse(User.FindFirstValue("DoctorId"));

            // Fetch doctor details from Doctors table
            using var db = _context.CreateConnection();
            var doctor = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT d.DoctorId, d.UserId, d.Name, d.Specialization, d.DepartmentId, u.Email " +
                "FROM Doctors d " +
                "JOIN Users u ON d.UserId = u.UserId " +
                "WHERE d.DoctorId = @DoctorId",
                new { DoctorId = doctorId });

            ViewBag.Doctor = doctor;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            int doctorId = int.Parse(User.FindFirstValue("DoctorId"));
            using var db = _context.CreateConnection();
            var doctor = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT d.DoctorId, d.UserId, d.Name, d.Specialization, d.DepartmentId, u.Email " +
                "FROM Doctors d " +
                "JOIN Users u ON d.UserId = u.UserId " +
                "WHERE d.DoctorId = @DoctorId",
                new { DoctorId = doctorId });

            return View(doctor);
        }
    }
}
