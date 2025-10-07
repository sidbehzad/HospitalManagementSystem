using HospitalManagementSystem.Dtos.MedicalRecord;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Repository.MedicalRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordsRepository _repo;

        public MedicalRecordController(IMedicalRecordsRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public IActionResult AddRecord(int patientId)
        {
            var model = new AddRecordDto { PatientId = patientId, RecordDate = DateTime.Now };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddRecord(AddRecordDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                int doctorId = int.Parse(User.FindFirstValue("DoctorId"));
                await _repo.AddMedicalRecordAsync(dto, doctorId);
                return RedirectToAction("ViewPatientHistory", new { patientId = dto.PatientId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["Error"] = "Unable to save medical record. Please try again.";
                return View(dto);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> ViewPatientHistory(int patientId)
        {
            try
            {
                var records = await _repo.GetPatientHistoryAsync(patientId);
                return View(records);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["Error"] = "Unable to fetch patient history.";
                return View(Enumerable.Empty<MedicalRecord>());
            }
        }
    }
}
