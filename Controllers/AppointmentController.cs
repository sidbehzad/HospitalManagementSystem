using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.Appointment;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;


namespace HospitalManagementSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly DapperContext _context;
        public AppointmentController(DapperContext dapper)
        {

            _context = dapper;
        }

        public async Task<IActionResult> MyTodayAppointments(int doctorId)
        {
            using var db = _context.CreateConnection();
            var sql = @"SELECT a.AppointmentId, p.Name AS PatientName, a.AppointmentDate, a.Status FROM Appointments a JOIN Patients p ON a.PatientId = p.PatientId WHERE a.DoctorId = @DoctorId AND CAST(a.AppointmentDate AS DATE) = CAST(@Today AS DATE)";

            var appointments = await db.QueryAsync<AppointmentDto>(
                sql,
                new
                {
                    DoctorId = doctorId,
                    Today = DateTime.UtcNow
                }
            );


            return PartialView(appointments);
        }


        public async Task<IActionResult> GetAllAppointments()
        {
            using var db = _context.CreateConnection();

            var result = await db.QueryAsync<Appointment>("SELECT AppointmentId, PatientId, DoctorId, AppointmentDate, Status FROM Appointments ");

            return PartialView(result);
        }

        public async Task<IActionResult> ApproveAppointment(int id)
        {
            using var db = _context.CreateConnection();

            try
            {


                var rowsAffected = await db.ExecuteAsync(
                    "UPDATE Appointments SET Status = @Status WHERE AppointmentId = @Id",
                    new { Status = "Approved", Id = id }
                );

                if (rowsAffected == 0)
                {
                    return NotFound();
                }

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while approving the appointment.");
            }
        }


        public async Task<IActionResult> DeclineAppointment(int id)
        {
            using var db = _context.CreateConnection();

            try
            {


                var rowsAffected = await db.ExecuteAsync(
                    "UPDATE Appointments SET Status = @Status WHERE AppointmentId = @Id",
                    new { Status = "Decline", Id = id }
                );

                if (rowsAffected == 0)
                {
                    return NotFound();
                }

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while approving the appointment.");
            }
        }
    


    //user


      [HttpGet]
        public IActionResult Book(int doctorId)
        {
            var model = new BookAppointmentDto
            {
                DoctorId = doctorId,
                AppointmentDate = DateTime.Now.AddDays(1) 
            };
            return View(model);
        }

        
        [HttpPost]
        public async Task<IActionResult> Book(BookAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            using var db = _context.CreateConnection();

            try
            {
                var appointmentId = 0;
                await db.ExecuteAsync(
                    "sp_BookAppointment",
                    new
                    {
                        PatientId = User.FindFirstValue("PatientId"),
                        DoctorId = dto.DoctorId,
                        AppointmentDate = dto.AppointmentDate,
                        AppointmentId = appointmentId
                    },
                    commandType: CommandType.StoredProcedure
                );

                return RedirectToAction("MyAppointments");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["Error"] = "Unable to book appointment.";
                return View(dto);
            }
        }

       
        [HttpGet]
        public async Task<IActionResult> MyAppointments()
        {
            using var db = _context.CreateConnection();
            var patientId = User.FindFirstValue("PatientId"); ;

            var appointments = await db.QueryAsync<dynamic>(
                @"SELECT a.AppointmentId, a.AppointmentDate, a.Status, 
                     d.Name AS DoctorName, d.Specialization
              FROM Appointments a
              JOIN Doctors d ON a.DoctorId = d.DoctorId
              WHERE a.PatientId = @PatientId
              ORDER BY a.AppointmentDate DESC",
                new { PatientId = patientId }
            );

            return View(appointments);
        }

       
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            using var db = _context.CreateConnection();

            try
            {
                await db.ExecuteAsync(
                    "UPDATE Appointments SET Status = @Status WHERE AppointmentId = @Id",
                    new { Status = "Cancelled", Id = id }
                );

                return RedirectToAction("MyAppointments");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["Error"] = "Unable to cancel appointment.";
                return RedirectToAction("MyAppointments");
            }
        }




        //user


        [HttpGet]
        public async Task<IActionResult> GetPatientRecords()
        {
            var patientId = User.FindFirstValue("PatientId");
            if (string.IsNullOrEmpty(patientId))
            {
                return Unauthorized(); 
            }

            using var db = _context.CreateConnection();

            var result = await db.QueryAsync<Appointment>(
                @"SELECT AppointmentId, DoctorId, PatientId, AppointmentDate, Status 
              FROM Appointments 
              WHERE PatientId = @PatientId",
                new { PatientId = patientId });

            var appointments = result.ToList();

            
            if (!appointments.Any())
            {
                ViewBag.Message = "No appointment records found.";
            }

            return View(appointments);
        }
    }
}