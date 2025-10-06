using HospitalManagementSystem.Repository.Users;
using HospitalManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    public class PatientDashboardController : Controller
    {
        private readonly IUsersRepository _repo;
        private readonly DapperContext _context;

        public PatientDashboardController(IUsersRepository repo, DapperContext context)
        {
            _repo = repo;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int patientId = int.Parse(User.FindFirstValue("PatientId"));

            using var db = _context.CreateConnection();
            var patient = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT p.PatientId, p.UserId, p.Name, u.Email " +
                "FROM Patients p " +
                "JOIN Users u ON p.UserId = u.UserId " +
                "WHERE p.PatientId = @PatientId",
                new { PatientId = patientId });

            ViewBag.Patient = patient;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            int patientId = int.Parse(User.FindFirstValue("PatientId"));
            using var db = _context.CreateConnection();
            var patient = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT p.PatientId, p.UserId, p.Name, u.Email " +
                "FROM Patients p " +
                "JOIN Users u ON p.UserId = u.UserId " +
                "WHERE p.PatientId = @PatientId",
                new { PatientId = patientId });

            return View(patient);
        }
    }
}
