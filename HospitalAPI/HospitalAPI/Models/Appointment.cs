namespace HospitalAPI.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int UserId { get; set; } // Patient ID
        public int DoctorId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; } // "Scheduled" or "Cancelled"
    }
}
