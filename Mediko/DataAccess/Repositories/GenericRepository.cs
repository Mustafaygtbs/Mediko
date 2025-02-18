using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Mediko.DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly MedikoDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(MedikoDbContext context)
        {
            _context = context;
        }

        public GenericRepository(MedikoDbContext context, DbSet<T> dbSet)
        {
            _context = context;
            _dbSet = dbSet;
        }

        public Task AddAsync(T entity, ChangeTracker changeTracker)
        {
            throw new NotImplementedException();
        }

        public void Delete(T entity, ChangeTracker changeTracker)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetAllAsync(ChangeTracker changeTracker)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate, ChangeTracker changeTracker)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetByIdAsync(int id, ChangeTracker changeTracker)
        {
            throw new NotImplementedException();
        }

        public void Update(T entity, ChangeTracker changeTracker)
        {
            throw new NotImplementedException();
        }
    }
}
