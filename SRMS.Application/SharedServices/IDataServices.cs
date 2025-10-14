using System.Linq.Expressions;

namespace SRMS.Application.SharedServices;

public interface IDataServices<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync(string id); // For Identity users with string IDs
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    // Write Operations
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(T entity);

    Task SaveChangesAsync();
}