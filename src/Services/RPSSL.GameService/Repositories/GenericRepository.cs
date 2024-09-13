using Microsoft.EntityFrameworkCore;
using RPSSL.GameService.Data;

namespace RPSSL.GameService.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public readonly ILogger<GenericRepository<T>> _logger;
        protected GameDbContext _context;
        internal DbSet<T> _dbSet;

        public GenericRepository(
            GameDbContext context,
            ILogger<GenericRepository<T>> logger
            )
        {
            _context = context;
            _logger = logger;

            _dbSet = context.Set<T>();
        }
        public virtual async Task<bool> Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
}
