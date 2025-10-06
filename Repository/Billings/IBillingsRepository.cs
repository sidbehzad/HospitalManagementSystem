using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Repository.Billings
{
    public interface IBillingsRepository
    {
        Task<IEnumerable<Bill>> GetPatientBillsAsync(int patientId);
        Task PayBillAsync(int billId);
        Task GenerateBillAsync(int patientId, decimal amount);
        Task<IEnumerable<Bill>> GetDoctorPatientsBillsAsync(int doctorId);
    }
}
