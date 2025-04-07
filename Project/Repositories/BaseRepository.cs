using Microsoft.EntityFrameworkCore;
using Project.Datas;
using System.Linq.Expressions;

namespace Project.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly TraditionalMedicineHospitalDbContext _context;
        private readonly DbSet<T> _dbSet;

        public BaseRepository(TraditionalMedicineHospitalDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }


        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T?> DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
            {
                return null;
            }
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T?> GetByNameAsync(string name)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<string>(e, "Name") == name);
        }

        public async Task<T?> GetByCodeAsync(string code)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<string>(e, "Code") == code);
        }

        public async Task<bool> IsCodeExistsAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            return await _dbSet.AnyAsync(e => EF.Property<string>(e, "Code") == code);
        }
    }
}