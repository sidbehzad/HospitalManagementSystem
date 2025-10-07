using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Repository.Billings
{
    public class BillingsRepository : IBillingsRepository
    {
        private readonly DapperContext _context;

        public BillingsRepository(DapperContext context)
        {
            _context = context;
        }

        
        public async Task<IEnumerable<Bill>> GetPatientBillsAsync(int patientId)
        {
            using var db = _context.CreateConnection();
            var sql = @"SELECT b.BillId, b.PatientId, b.Amount, b.PaymentStatus, b.BillDate, p.Name AS PatientName
                        FROM Billing b
                        JOIN Patients p ON b.PatientId = p.PatientId
                        WHERE b.PatientId=@PatientId
                        ORDER BY b.BillDate DESC";
            return await db.QueryAsync<Bill>(sql, new { PatientId = patientId });
        }

        
        public async Task PayBillAsync(int billId)
        {
            using var db = _context.CreateConnection();
            var sql = @"UPDATE Billing SET PaymentStatus='Paid' WHERE BillId=@BillId";
            await db.ExecuteAsync(sql, new { BillId = billId });
        }

        
        public async Task GenerateBillAsync(int patientId, decimal amount)
        {
            using var db = _context.CreateConnection();
            var sql = @"INSERT INTO Billing (PatientId, Amount, PaymentStatus, BillDate)
                        VALUES (@PatientId, @Amount, 'Pending', @BillDate)";
            await db.ExecuteAsync(sql, new { PatientId = patientId, Amount = amount, BillDate = DateTime.Now });
        }

        
        public async Task<IEnumerable<Bill>> GetDoctorPatientsBillsAsync(int doctorId)
        {
            using var db = _context.CreateConnection();
            var sql = @"
SELECT DISTINCT b.BillId, b.PatientId, b.Amount, b.PaymentStatus, b.BillDate, p.Name AS PatientName
FROM Billing b
JOIN Patients p ON b.PatientId = p.PatientId
JOIN Appointments a ON a.PatientId = b.PatientId
WHERE a.DoctorId = @DoctorId
ORDER BY b.BillDate DESC";
            return await db.QueryAsync<Bill>(sql, new { DoctorId = doctorId });
        }

        public async Task<IEnumerable<Bill>> GetAllBillsAsync()
        {
            using var db = _context.CreateConnection();
            var sql = "SELECT b.BillId, b.PatientId, b.Amount, b.PaymentStatus, b.BillDate, p.Name AS PatientName " +
                      "FROM Billing b JOIN Patients p ON b.PatientId = p.PatientId ORDER BY b.BillDate DESC";
            return await db.QueryAsync<Bill>(sql);
        }

    }
}
