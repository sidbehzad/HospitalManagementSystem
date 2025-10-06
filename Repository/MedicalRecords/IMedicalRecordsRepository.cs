using HospitalManagementSystem.Dtos.MedicalRecord;
using HospitalManagementSystem.Models;
namespace HospitalManagementSystem.Repository.MedicalRecords
{
    public interface IMedicalRecordsRepository
    {
        Task AddMedicalRecordAsync(AddRecordDto dto, int doctorId);
        Task<IEnumerable<MedicalRecord>> GetPatientHistoryAsync(int patientId);
    }
}
