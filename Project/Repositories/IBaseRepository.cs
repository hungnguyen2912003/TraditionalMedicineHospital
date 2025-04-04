using System.Linq.Expressions;

namespace Project.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T> FindAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T?> DeleteAsync(Guid id);

        Task<T?> GetByNameAsync(string name);
        Task<T?> GetByCodeAsync(string code);

        Task<bool> IsCodeExistsAsync(string code);
    }
}
