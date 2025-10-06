using HospitalManagementSystem.Dtos.Appointment;
using HospitalManagementSystem.Models;
namespace HospitalManagementSystem.Repository.Appointments
{
    public interface IAppointmentsRepository
    {
        Task<IEnumerable<AppointmentDto>> GetTodayAppointmentsAsync(int doctorId);
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<bool> ApproveAppointmentAsync(int appointmentId);
        Task<bool> DeclineAppointmentAsync(int appointmentId);
        Task<bool> CancelAppointmentAsync(int appointmentId);
        Task BookAppointmentAsync(int patientId, BookAppointmentDto dto);
        Task<IEnumerable<dynamic>> GetMyAppointmentsAsync(int patientId);
        Task<IEnumerable<Appointment>> GetPatientRecordsAsync(int patientId);
    }
}
