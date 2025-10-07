using HospitalManagementSystem.Dtos.Appointment;
using HospitalManagementSystem.Models;
namespace HospitalManagementSystem.Repository.Appointments
{
    public interface IAppointmentsRepository
    {
        // Doctor: Get today's appointments (optionally only approved)
        Task<IEnumerable<AppointmentDto>> GetTodayAppointmentsAsync(int doctorId, bool onlyApproved = false);

        // Doctor: Get all appointments for a doctor
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsForDoctorAsync(int doctorId);

        // Approve / Decline / Cancel appointments
        Task<bool> ApproveAppointmentAsync(int appointmentId);
        Task<bool> DeclineAppointmentAsync(int appointmentId);
        Task<bool> CancelAppointmentAsync(int appointmentId);

        // Book an appointment (Patient)
        Task BookAppointmentAsync(int patientId, BookAppointmentDto dto);

        // Patient: Get all appointments
        Task<IEnumerable<AppointmentDto>> GetMyAppointmentsAsync(int patientId);

        // Patient: Get medical records / past appointments
        Task<IEnumerable<Appointment>> GetPatientRecordsAsync(int patientId);
    }
}
