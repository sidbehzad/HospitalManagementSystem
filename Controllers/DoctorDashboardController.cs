using HospitalManagementSystem.Dtos.UserDtos;
using HospitalManagementSystem.Repository.Appointments;
using HospitalManagementSystem.Repository.Departments;
using HospitalManagementSystem.Repository.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Data.SqlClient;
namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorDashboardController : Controller
    {
        private readonly IUsersRepository _usersRepo;
        private readonly IAppointmentsRepository _appointmentsRepo;
        private readonly IDepartmentsRepository _departmentsRepo;

        public DoctorDashboardController(
            IUsersRepository usersRepo,
            IAppointmentsRepository appointmentsRepo,
            IDepartmentsRepository departmentsRepo)
        {
            _usersRepo = usersRepo;
            _appointmentsRepo = appointmentsRepo;
            _departmentsRepo = departmentsRepo;
        }

        // Doctor dashboard
        public async Task<IActionResult> Index()
        {
            int doctorId = int.Parse(User.FindFirstValue("DoctorId"));

            
            var doctor = await _usersRepo.GetDoctorProfileAsync(doctorId);
            ViewBag.Doctor = doctor;

            
            var todaysAppointments = await _appointmentsRepo.GetTodayAppointmentsAsync(doctorId, onlyApproved: true);
            ViewBag.TodaysAppointments = todaysAppointments.ToList();

           
            var allAppointments = await _appointmentsRepo.GetAllAppointmentsForDoctorAsync(doctorId);
            ViewBag.AllAppointments = allAppointments.ToList();

            return View();
        }

      
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            int doctorId = int.Parse(User.FindFirstValue("DoctorId"));
            var doctor = await _usersRepo.GetDoctorProfileAsync(doctorId);

            
            var departments = await _departmentsRepo.GetAllAsync();
            ViewBag.Departments = departments
                .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.Name })
                .ToList();

            return View(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditDoctorDto dto)
        {
            if (!ModelState.IsValid)
            {
                var departments = await _departmentsRepo.GetAllAsync();
                ViewBag.Departments = departments
                    .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.Name })
                    .ToList();
                return View(dto);
            }

            try
            {
                var updated = await _usersRepo.UpdateDoctorProfileAsync(dto);
                if (!updated)
                {
                    ModelState.AddModelError("", "Update failed. Please try again.");
                    var departments = await _departmentsRepo.GetAllAsync();
                    ViewBag.Departments = departments
                        .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.Name })
                        .ToList();
                    return View(dto);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                
                if (ex.Message.Contains("UNIQUE") || ex.Message.Contains("duplicate"))
                {
                    ModelState.AddModelError("Email", "This email is already registered. Please use a different one.");
                }
                else
                {
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }

              
                var departments = await _departmentsRepo.GetAllAsync();
                ViewBag.Departments = departments
                    .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.Name })
                    .ToList();

                return View(dto);
            }
        }



    }
}
