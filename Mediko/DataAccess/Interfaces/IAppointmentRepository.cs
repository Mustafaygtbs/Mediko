using Mediko.Entities;

namespace Mediko.DataAccess.Interfaces
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        //  belirli bir user ve tarih aralığına göre randevuları getir
        Task<IEnumerable<Appointment>> GetAppointmentsByUserAndDateAsync(
            string userId, DateTime startDate, DateTime endDate);

        //  onaylanmış randevuları getir
        Task<IEnumerable<Appointment>> GetConfirmedAppointmentsAsync();

    }
}
