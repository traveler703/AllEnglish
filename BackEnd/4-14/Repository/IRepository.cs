// C#
public interface IRepository<T>
{
    Task<T> GetByIdAsync(long id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(long id);
}

