using Microsoft.EntityFrameworkCore;
using SmartData;
using SmartInterfaces.IRepository;
using System.Linq.Expressions;

namespace SmartRepository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _Context;
        private readonly DbSet<T> _DbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _Context = context;
            _DbSet = _Context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _DbSet.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _DbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _DbSet.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _DbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _DbSet.FindAsync(id);
        }

        public async Task SaveChangeAsync()
        {
            await _Context.SaveChangesAsync(); 
        }

        public void Update(T entity)
        {
            _Context.Update(entity);
        }
    }
}
