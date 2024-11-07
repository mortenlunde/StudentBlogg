using System.Linq.Expressions;

namespace StudentBlogg.Common.Interfaces;

public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> AddAsync(T entity);
    Task<T?> UpdateByIdAsync(Guid id, T entity);
    Task<T?> DeleteByIdAsync(Guid id);
}