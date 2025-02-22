namespace Mediko.Entities.DTOs.AppointmentDTOs
{
    namespace Mediko.Entities.DTOs.AppointmentDTOs
    {
        public class AppointmentCreateDto
        {
            public int PoliclinicTimeslotId { get; set; }
            public int PoliclinicId { get; set; }
            public string UserId { get; set; } = string.Empty;
            public DateOnly AppointmentDate { get; set; }
            public TimeOnly AppointmentTime { get; set; }
            public AppointmentStatus Status { get; set; }
        }
    }

}
