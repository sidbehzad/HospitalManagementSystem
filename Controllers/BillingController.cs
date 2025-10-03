using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace HospitalManagementSystem.Controllers
{
    public class BillingController : Controller
    {
        private readonly DapperContext _context;

        public BillingController(DapperContext context)
        {
            _context = context;
        }

      
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyBills()
        {
            int patientId = GetLoggedInPatientId();

            using var connection = _context.CreateConnection();
            var bills = await connection.QueryAsync<Bill>(
                "SELECT * FROM Billing WHERE PatientId = @PatientId ORDER BY BillDate DESC",
                new { PatientId = patientId });

            return View(bills);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> PayBill(int billId)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE Billing SET PaymentStatus = 'Paid' WHERE BillId = @BillId",
                new { BillId = billId });

            return RedirectToAction("MyBills");
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
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                @"INSERT INTO Billing (PatientId, Amount, PaymentStatus, BillDate)
              VALUES (@PatientId, @Amount, 'Pending', @BillDate)",
                new { PatientId = patientId, Amount = amount, BillDate = DateTime.Now });

            return RedirectToAction("ViewMyPatientsBills");
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ViewMyPatientsBills()
        {
            int doctorId = GetLoggedInDoctorId();

            using var connection = _context.CreateConnection();
            var bills = await connection.QueryAsync<Bill>(
                @"SELECT b.BillId, b.PatientId, b.Amount, b.PaymentStatus, b.BillDate, p.Name AS PatientName
              FROM Billing b
              JOIN Patients p ON b.PatientId = p.PatientId
              JOIN DoctorPatients dp ON dp.PatientId = b.PatientId
              WHERE dp.DoctorId = @DoctorId
              ORDER BY b.BillDate DESC",
                new { DoctorId = doctorId });

            return View(bills);
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
