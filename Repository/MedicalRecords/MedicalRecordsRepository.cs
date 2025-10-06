using Dapper;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Dtos.MedicalRecord;
using HospitalManagementSystem.Models;
using System.Data;

namespace HospitalManagementSystem.Repository.MedicalRecords
{
    public class MedicalRecordsRepository: IMedicalRecordsRepository
    {
        private readonly DapperContext _context;

        public MedicalRecordsRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task AddMedicalRecordAsync(AddRecordDto dto, int doctorId)
        {
            using var db = _context.CreateConnection();

            await db.ExecuteAsync(
                "sp_AddMedicalRecord",
                new
                {
                    PatientId = dto.PatientId,
                    DoctorId = doctorId,
                    Diagnosis = dto.Diagnosis,
                    Treatment = dto.Treatment,
                    RecordDate = dto.RecordDate
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<MedicalRecord>> GetPatientHistoryAsync(int patientId)
        {
            using var db = _context.CreateConnection();

            var records = await db.QueryAsync<MedicalRecord>(
                "sp_GetPatientHistory",
                new { PatientId = patientId },
                commandType: CommandType.StoredProcedure
            );

            return records;
        }

    }
}
