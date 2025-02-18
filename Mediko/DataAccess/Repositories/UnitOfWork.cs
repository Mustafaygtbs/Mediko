using Mediko.Entities;

namespace Mediko.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MedikoDbContext _context;


        public IGenericRepository<Policlinic> PoliclinicRepository { get; }
        public IGenericRepository<Appointment> AppointmentRepository { get; }


        public UnitOfWork(MedikoDbContext context)
        {
            _context = context;

            PoliclinicRepository = new GenericRepository<Policlinic>(_context);
            AppointmentRepository = new GenericRepository<Appointment>(_context);
        }

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
