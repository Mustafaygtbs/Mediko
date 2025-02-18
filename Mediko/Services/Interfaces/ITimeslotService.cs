namespace Mediko.Services.Interfaces
{
    public interface ITimeslotService
    {
        Task GenerateTimeslotsForNextDaysAsync(int days);
        Task RemoveOldTimeslotsAsync(int daysThreshold);
    }
}
