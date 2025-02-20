namespace Mediko.Entities.DTOs.AppointmentDTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int PoliclinicTimeslotId { get; set; }
        public int PoliclinicId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly AppointmentTime { get; set; }
        public DateTime FullAppointmentDateTime { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
