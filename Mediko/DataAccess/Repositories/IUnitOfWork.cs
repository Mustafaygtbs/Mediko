using Mediko.Entities;

namespace Mediko.DataAccess.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Policlinic> PoliclinicRepository { get; }
        IGenericRepository<Appointment> AppointmentRepository { get; }
       

        Task<int> Save();
    }
}
