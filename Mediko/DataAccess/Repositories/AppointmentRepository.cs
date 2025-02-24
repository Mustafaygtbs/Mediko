using Mediko.DataAccess.Interfaces;
using Mediko.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mediko.DataAccess.Repositories
{
    public class AppointmentRepository
          : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(MedikoDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByUserAndDateAsync(
            string userId, DateTime startDate, DateTime endDate)
        {
            // Randevu tarihini FullAppointmentDateTime veya AppointmentDate baz alabilirsin
            return await _dbSet
                .Where(a => a.UserId == userId
                    && a.FullAppointmentDateTime >= startDate
                    && a.FullAppointmentDateTime <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetConfirmedAppointmentsAsync()
        {
            return await _dbSet
                 .Where(a => a.Status == AppointmentStatus.Onaylandı)
                 .ToListAsync();
        } 
        public async Task<IEnumerable<Appointment>> GetOnayBekleyen()
        {
            return await _dbSet
                 .Where(a => a.Status == AppointmentStatus.OnayBekliyor)
                 .ToListAsync();
        }

    }
}
