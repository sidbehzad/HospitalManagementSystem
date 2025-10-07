using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.Appointment;
using HospitalManagementSystem.Models;
using System.Data;

namespace HospitalManagementSystem.Repository.Appointments
{
    public class AppointmentsRepository : IAppointmentsRepository
    {
        private readonly DapperContext _context;

        public AppointmentsRepository(DapperContext context)
        {
            _context = context;
        }

        
        public async Task<IEnumerable<AppointmentDto>> GetTodayAppointmentsAsync(int doctorId, bool onlyApproved = false)
        {
            using var db = _context.CreateConnection();
            var sql = @"
SELECT a.AppointmentId,
       a.PatientId,             -- Add this
       a.DoctorId,              -- Optional
       p.Name AS PatientName,
       d.Name AS DoctorName,
       a.AppointmentDate,
       a.Status
FROM Appointments a
JOIN Patients p ON a.PatientId = p.PatientId
JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE a.DoctorId = @DoctorId AND CAST(a.AppointmentDate AS DATE) = CAST(@Today AS DATE)";


            if (onlyApproved)
            {
                sql += " AND a.Status='Approved'";
            }

            return await db.QueryAsync<AppointmentDto>(sql, new { DoctorId = doctorId, Today = DateTime.UtcNow });
        }

        
        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsForDoctorAsync(int doctorId)
        {
            using var db = _context.CreateConnection();
            var sql = @"
SELECT a.AppointmentId, 
       p.Name AS PatientName, 
       d.Name AS DoctorName, 
       a.AppointmentDate, 
       a.Status,
       a.PatientId
FROM Appointments a
JOIN Patients p ON a.PatientId = p.PatientId
JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE a.DoctorId = @DoctorId
ORDER BY a.AppointmentDate DESC";

            return await db.QueryAsync<AppointmentDto>(sql, new { DoctorId = doctorId });
        }

        
        public async Task<bool> ApproveAppointmentAsync(int appointmentId)
        {
            using var db = _context.CreateConnection();
            var rows = await db.ExecuteAsync(
                "UPDATE Appointments SET Status=@Status WHERE AppointmentId=@Id",
                new { Status = "Approved", Id = appointmentId });
            return rows > 0;
        }

       
        public async Task<bool> DeclineAppointmentAsync(int appointmentId)
        {
            using var db = _context.CreateConnection();
            var rows = await db.ExecuteAsync(
                "UPDATE Appointments SET Status=@Status WHERE AppointmentId=@Id",
                new { Status = "Declined", Id = appointmentId });
            return rows > 0;
        }

       
        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            using var db = _context.CreateConnection();
            var rows = await db.ExecuteAsync(
                "UPDATE Appointments SET Status=@Status WHERE AppointmentId=@Id",
                new { Status = "Cancelled", Id = appointmentId });
            return rows > 0;
        }

        
        public async Task BookAppointmentAsync(int patientId, BookAppointmentDto dto)
        {
            using var db = _context.CreateConnection();
            await db.ExecuteAsync("sp_BookAppointment",
                new
                {
                    PatientId = patientId,
                    DoctorId = dto.DoctorId,
                    AppointmentDate = dto.AppointmentDate,
                    AppointmentId = 0
                },
                commandType: CommandType.StoredProcedure);
        }

       
        public async Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(int patientId)
        {
            using var db = _context.CreateConnection();
            var sql = @"
SELECT a.AppointmentId, 
       a.AppointmentDate, 
       a.Status, 
       d.Name AS DoctorName, 
       d.Specialization,
       a.PatientId
FROM Appointments a
JOIN Doctors d ON a.DoctorId = d.DoctorId
WHERE a.PatientId=@PatientId
ORDER BY a.AppointmentDate DESC";

            return await db.QueryAsync<AppointmentDto>(sql, new { PatientId = patientId });
        }

        
        public async Task<IEnumerable<Appointment>> GetPatientRecordsAsync(int patientId)
        {
            using var db = _context.CreateConnection();
            return await db.QueryAsync<Appointment>(
                @"SELECT AppointmentId, DoctorId, PatientId, AppointmentDate, Status 
                  FROM Appointments 
                  WHERE PatientId=@PatientId",
                new { PatientId = patientId });
        }
    }
}
