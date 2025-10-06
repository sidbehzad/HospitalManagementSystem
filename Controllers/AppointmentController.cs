using HospitalManagementSystem.Dtos.Appointment;
using HospitalManagementSystem.Repository.Appointments;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentsRepository _repo;

        public AppointmentController(IAppointmentsRepository repo)
        {
            _repo = repo;
        }

        // Doctor views today's appointments
        public async Task<IActionResult> MyTodayAppointments(int doctorId)
        {
            var appointments = await _repo.GetTodayAppointmentsAsync(doctorId);
            return PartialView(appointments);
        }

        public async Task<IActionResult> GetAllAppointments()
        {
            var result = await _repo.GetAllAppointmentsAsync();
            return PartialView(result);
        }

        public async Task<IActionResult> ApproveAppointment(int id)
        {
            try
            {
                if (!await _repo.ApproveAppointmentAsync(id)) return NotFound();
                return RedirectToAction("Dashboard");
            }
            catch
            {
                return StatusCode(500, "Error approving appointment.");
            }
        }

        public async Task<IActionResult> DeclineAppointment(int id)
        {
            try
            {
                if (!await _repo.DeclineAppointmentAsync(id)) return NotFound();
                return RedirectToAction("Dashboard");
            }
            catch
            {
                return StatusCode(500, "Error declining appointment.");
            }
        }

        [HttpGet]
        public IActionResult Book(int doctorId) => View(new BookAppointmentDto
        {
            DoctorId = doctorId,
            AppointmentDate = DateTime.Now.AddDays(1)
        });

        [HttpPost]
        public async Task<IActionResult> Book(BookAppointmentDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var patientId = int.Parse(User.FindFirstValue("PatientId"));
                await _repo.BookAppointmentAsync(patientId, dto);
                return RedirectToAction("MyAppointments");
            }
            catch
            {
                TempData["Error"] = "Unable to book appointment.";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyAppointments()
        {
            var patientId = int.Parse(User.FindFirstValue("PatientId"));
            var appointments = await _repo.GetMyAppointmentsAsync(patientId);
            return View(appointments);
        }

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
