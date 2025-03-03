﻿using Mediko.Entities.DTOs.UserDTOs;

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
            public AppointmentStatus Status { get; set; }
        }
    }

}
