namespace StudentBlogg.Common.Interfaces;

public interface IBaseService<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<T?> AddAsync(T entity);
    Task<T?> UpdateAsync(Guid id, T entity);
    Task<T?> DeleteByIdAsync(Guid id);
}