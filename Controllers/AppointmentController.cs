using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.Appointment;
using HospitalManagementSystem.Repository.Appointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentsRepository _repo;
        private readonly DapperContext _context;

        public AppointmentController(IAppointmentsRepository repo, DapperContext context)
        {
            _repo = repo;
            _context = context;
        }

        
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MyTodayAppointments(int doctorId)
        {
            var appointments = await _repo.GetTodayAppointmentsAsync(doctorId);
            return PartialView(appointments);
        }


        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllAppointments()
        {
            int doctorId = int.Parse(User.FindFirstValue("DoctorId"));
            var appointments = await _repo.GetAllAppointmentsForDoctorAsync(doctorId);
            return PartialView(appointments);
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            try
            {
                if (!await _repo.ApproveAppointmentAsync(id)) return NotFound();
                return RedirectToAction("Index","DoctorDashboard");
            }
            catch
            {
                return StatusCode(500, "Error approving appointment.");
            }
        }


        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DeclineAppointment(int id)
        {
            try
            {
                if (!await _repo.DeclineAppointmentAsync(id)) return NotFound();
                return RedirectToAction("Index", "DoctorDashboard");
            }
            catch
            {
                return StatusCode(500, "Error declining appointment.");
            }
        }



        [HttpGet]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Book()
        {
            await PopulateDoctorsDropdownAsync();
            return View(new BookAppointmentDto
            {
                AppointmentDate = DateTime.Now.AddDays(1)
            });
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Book(BookAppointmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDoctorsDropdownAsync(); 
                return View(dto);
            }

            try
            {
                var patientId = int.Parse(User.FindFirstValue("PatientId"));
                await _repo.BookAppointmentAsync(patientId, dto);
                return RedirectToAction("MyAppointments");
            }
            catch
            {
                TempData["Error"] = "Unable to book appointment.";
                await PopulateDoctorsDropdownAsync(); 
                return View(dto);
            }
        }

        
        private async Task PopulateDoctorsDropdownAsync()
        {
            using var db = _context.CreateConnection();
            var doctors = await db.QueryAsync<dynamic>(@"
        SELECT DoctorId, Name, Specialization 
        FROM Doctors
    ");

            ViewBag.Doctors = doctors.Select(d => new SelectListItem
            {
                Value = d.DoctorId.ToString(),
                Text = $"{d.Name} ({d.Specialization})"
            }).ToList();
        }

        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> MyAppointments()
        {
            var patientId = int.Parse(User.FindFirstValue("PatientId"));
            var appointments = await _repo.GetMyAppointmentsAsync(patientId);
            return View(appointments);
        }


        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _repo.CancelAppointmentAsync(id);
                return RedirectToAction("MyAppointments");
            }
            catch
            {
                TempData["Error"] = "Unable to cancel appointment.";
                return RedirectToAction("MyAppointments");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> GetPatientRecords()
        {
            var patientIdStr = User.FindFirstValue("PatientId");
            if (string.IsNullOrEmpty(patientIdStr)) return Unauthorized();

            var patientId = int.Parse(patientIdStr);
            var appointments = await _repo.GetPatientRecordsAsync(patientId);

            if (!appointments.Any()) ViewBag.Message = "No appointment records found.";

            return View(appointments);
        }
    }
}
