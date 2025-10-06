using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Repository.Billings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingsRepository _repo;

        public BillingController(IBillingsRepository repo)
        {
            _repo = repo;
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyBills()
        {
            try
            {
                int patientId = GetLoggedInPatientId();
                var bills = await _repo.GetPatientBillsAsync(patientId);
                return View(bills);
            }
            catch
            {
                return StatusCode(500, "Unable to fetch bills.");
            }
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> PayBill(int billId)
        {
            try
            {
                await _repo.PayBillAsync(billId);
                return RedirectToAction("MyBills");
            }
            catch
            {
                return StatusCode(500, "Unable to pay bill.");
            }
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet]
        public IActionResult GenerateBill(int patientId)
        {
            ViewBag.PatientId = patientId;
            return View();
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> GenerateBill(int patientId, decimal amount)
        {
            try
            {
                await _repo.GenerateBillAsync(patientId, amount);
                return RedirectToAction("ViewMyPatientsBills");
            }
            catch
            {
                return StatusCode(500, "Unable to generate bill.");
            }
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ViewMyPatientsBills()
        {
            try
            {
                int doctorId = GetLoggedInDoctorId();
                var bills = await _repo.GetDoctorPatientsBillsAsync(doctorId);
                return View(bills);
            }
            catch
            {
                return StatusCode(500, "Unable to fetch patients' bills.");
            }
        }

        private int GetLoggedInPatientId()
        {
            var claim = User.FindFirst("PatientId") ?? throw new Exception("PatientId claim not found.");
            return int.Parse(claim.Value);
        }

        private int GetLoggedInDoctorId()
        {
            var claim = User.FindFirst("DoctorId") ?? throw new Exception("DoctorId claim not found.");
            return int.Parse(claim.Value);
        }


    }
}
