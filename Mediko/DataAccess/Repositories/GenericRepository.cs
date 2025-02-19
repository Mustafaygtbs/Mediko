using Mediko.DataAccess.Interfaces;
using Mediko.Entities;
using Mediko.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Mediko.DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly MedikoDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(MedikoDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Eklenecek varlık boş olamaz.");

            await _dbSet.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Silinecek varlık boş olamaz.");
            _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate), "Filtreleme koşulu boş olamaz.");

            return await _dbSet.Where(predicate).ToListAsync();
        }

        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Güncellenecek varlık boş olamaz.");

            _dbSet.Update(entity);
        }

        async Task<T> IGenericRepository<T>.GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID sıfırdan büyük olmalıdır.", nameof(id));

            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                throw new NotFoundException($"ID: {id} ile kayıt bulunamadı.");

            return entity;
        }
    }
}
