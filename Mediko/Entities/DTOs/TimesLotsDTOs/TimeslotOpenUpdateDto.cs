namespace Mediko.Entities.DTOs.TimesLotsDTOs
{
    public class TimeslotOpenUpdateDto
    {
        public int PoliclinicId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public bool IsOpen { get; set; }
    }
}
