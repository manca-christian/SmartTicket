using SmartTicket.Application.Specifications;

namespace SmartTicket.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec);
    Task<int> CountAsync(ISpecification<T> spec);
    Task<List<T>> ListAsync(ISpecification<T> spec);
    Task AddAsync(T entity);
    Task SaveChangesAsync();
}
