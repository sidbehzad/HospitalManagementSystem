using HospitalManagementSystem.Dtos.UserDtos;
using HospitalManagementSystem.Repository.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientDashboardController : Controller
    {
        private readonly IUsersRepository _repo;

        public PatientDashboardController(IUsersRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            int patientId = int.Parse(User.FindFirstValue("PatientId"));
            var patient = await _repo.GetPatientProfileAsync(patientId);
            ViewBag.Patient = patient;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            int patientId = int.Parse(User.FindFirstValue("PatientId"));
            var patient = await _repo.GetPatientProfileAsync(patientId);

            var genders = new List<SelectListItem>
    {
        new SelectListItem { Text = "Male", Value = "Male", Selected = (patient.Gender=="Male") },
        new SelectListItem { Text = "Female", Value = "Female", Selected = (patient.Gender=="Female") }
    };
            ViewBag.Genders = genders;

            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditPatientDto dto)
        {
            
            var genders = new List<SelectListItem>
    {
        new SelectListItem { Text = "Male", Value = "Male", Selected = (dto.Gender=="Male") },
        new SelectListItem { Text = "Female", Value = "Female", Selected = (dto.Gender=="Female") }
    };
            ViewBag.Genders = genders;

            if (!ModelState.IsValid) return View(dto);

            try
            {
                bool updated = await _repo.UpdatePatientProfileAsync(dto);
                if (!updated)
                {
                    ModelState.AddModelError("", "Update failed. Please try again.");
                    return View(dto);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                
                if (ex.Message.Contains("Violation") || ex.Message.Contains("duplicate"))
                {
                    ModelState.AddModelError("Email", "Email already exists. Please use a different email.");
                }
                else
                {
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
                return View(dto);
            }
        }

    }
}
