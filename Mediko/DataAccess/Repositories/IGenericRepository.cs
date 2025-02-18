using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Mediko.DataAccess.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id, ChangeTracker changeTracker);
        Task<IEnumerable<T>> GetAllAsync(ChangeTracker changeTracker);
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate, ChangeTracker changeTracker);

        Task AddAsync(T entity, ChangeTracker changeTracker);
        void Update(T entity,ChangeTracker changeTracker);
        void Delete(T entity, ChangeTracker changeTracker);


    }
}
