namespace Mediko.Entities.DTOs.TimesLotsDTOs
{
    public class TimeslotBookedUpdateDto
    {
        public int PoliclinicId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public bool IsBooked { get; set; }
    }
}
