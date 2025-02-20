namespace Mediko.Entities.DTOs.AppointmentDTOs
{
    namespace Mediko.Entities.DTOs.AppointmentDTOs
    {
        public class AppointmentUpdateDto
        {
            public int PoliclinicTimeslotId { get; set; }
            public int PoliclinicId { get; set; }
            public DateOnly AppointmentDate { get; set; }
            public TimeOnly AppointmentTime { get; set; }
            public bool IsConfirmed { get; set; }
        }
    }

}
