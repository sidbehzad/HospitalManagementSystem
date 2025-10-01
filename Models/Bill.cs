namespace HospitalManagementSystem.Models
{
    public class Bill
    {
        public int BillId { get; set; }
        public int PatientId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = null!; // Paid / Pending
        public DateTime BillDate { get; set; }
    }
}
