using System.Text.Json.Serialization;

namespace Mediko.Entities.DTOs.AppointmentDTOs
{
    public class AppointmentConfirmUpdateDto
    {
        public string OgrenciNo { get; set; } = string.Empty;
        public int PoliclinicId { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly AppointmentTime { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AppointmentStatus Status { get; set; }
    }
}
