using Domain.Entity;

namespace Domain.Repository
{
    public interface IRepository<T> where T : EntityBase
    {
        Task<IList<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
