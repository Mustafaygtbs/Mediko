using Mediko.DataAccess.Interfaces;
using Mediko.Entities;

namespace Mediko.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MedikoDbContext _context;

        public UnitOfWork(MedikoDbContext context, IGenericRepository<Policlinic> policlinicRepository, IGenericRepository<Appointment> appointmentRepository, IGenericRepository<Department> departmentRepository)
        {
            _context = context;
            PoliclinicRepository = policlinicRepository;
            AppointmentRepository = new AppointmentRepository(_context);
            DepartmentRepository = departmentRepository;
        }

        public IGenericRepository<Policlinic> PoliclinicRepository { get; }
        public IAppointmentRepository AppointmentRepository { get; private set; }

        public IGenericRepository<Department> DepartmentRepository { get; }



        public async Task<int> Save()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
