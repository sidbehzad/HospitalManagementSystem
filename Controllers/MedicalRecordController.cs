using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.MedicalRecord;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    public class MedicalRecordController : Controller
    {
        private readonly DapperContext _context;
        public MedicalRecordController(DapperContext dapper)
        {

            _context = dapper;
        }


        [HttpGet]
        public IActionResult AddRecord(int patientId)
        {
            var model = new AddRecordDto { PatientId = patientId, RecordDate = DateTime.Now };
            return View(model);
        }


        public async Task<IActionResult> AddRecord(AddRecordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            using var db = _context.CreateConnection();

            try
            {
                await db.ExecuteAsync(
                    "sp_AddMedicalRecord",
                    new
                    {
                        PatientId = dto.PatientId,
                        DoctorId = User.FindFirstValue("DoctorId"),
                        Diagnosis = dto.Diagnosis,
                        Treatment = dto.Treatment,
                        RecordDate = dto.RecordDate
                    },
                    commandType: CommandType.StoredProcedure
                );

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
        public async Task<IActionResult> ViewPatientHistory(int patientId)
        {
            using var db = _context.CreateConnection();

            var records = await db.QueryAsync<MedicalRecord>(
                "sp_GetPatientHistory",
                new { PatientId = patientId },
                commandType: CommandType.StoredProcedure
            );

            return View(records);
        }


    }
}