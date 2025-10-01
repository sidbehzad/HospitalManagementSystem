using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly DapperContext _context;
        public AdminController(DapperContext dapper)
        {
            
            _context = dapper;
        }
        public async Task<IActionResult> AdminDashBoard()
        {
            using var db = _context.CreateConnection();
            var doctorCount =await db.QueryAsync<Doctor>("Select Count(DoctorId) from Doctors");
            var patientCount =await db.QueryAsync<Doctor>("Select Count(PatientId) from patient");



            return View();
        }
    }
}
