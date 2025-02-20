using Mediko.Entities;

namespace Mediko.DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Department> DepartmentRepository { get; }
        IGenericRepository<Policlinic> PoliclinicRepository { get; }
        IAppointmentRepository AppointmentRepository { get; }



        Task<int> Save();
    }
}
