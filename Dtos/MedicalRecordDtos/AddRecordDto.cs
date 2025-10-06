namespace HospitalManagementSystem.Dtos.MedicalRecord
{
    public class AddRecordDto
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public DateTime RecordDate { get; set; }
    }
}
