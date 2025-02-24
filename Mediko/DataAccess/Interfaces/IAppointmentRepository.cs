using Mediko.Entities;

namespace Mediko.DataAccess.Interfaces
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetAppointmentsByUserAndDateAsync(
            string userId, DateTime startDate, DateTime endDate);


        Task<IEnumerable<Appointment>> GetConfirmedAppointmentsAsync();
        Task<IEnumerable<Appointment>> GetOnayBekleyen();

    }
}
