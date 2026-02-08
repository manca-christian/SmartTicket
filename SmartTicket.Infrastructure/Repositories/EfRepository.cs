using Microsoft.EntityFrameworkCore;
using SmartTicket.Application.Interfaces;
using SmartTicket.Application.Specifications;
using SmartTicket.Domain.Entities;
using SmartTicket.Infrastructure.Persistence;
using SmartTicket.Infrastructure.Specifications;
using System.Linq.Expressions;

namespace SmartTicket.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _db;
    public EfRepository(AppDbContext db) => _db = db;

    public Task AddAsync(T entity)
    {
        _db.Set<T>().Add(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec)
        => await ApplySpecification(spec).FirstOrDefaultAsync();

    public async Task<List<T>> ListAsync(ISpecification<T> spec)
        => await ApplySpecification(spec).AsNoTracking().ToListAsync();

    public async Task<int> CountAsync(ISpecification<T> spec)
        => await ApplySpecification(spec, applyPaging: false).CountAsync();

    private IQueryable<T> ApplySpecification(ISpecification<T> spec, bool applyPaging = true)
    {
        var baseQuery = _db.Set<T>().AsQueryable();

        if (!applyPaging && spec is BaseSpecification<T> bs)
        {
            var noPaging = new NoPagingSpecification<T>(bs);
            return SpecificationEvaluator.GetQuery(baseQuery, noPaging);
        }

        return SpecificationEvaluator.GetQuery(baseQuery, spec);
    }

    private sealed class NoPagingSpecification<TX> : ISpecification<TX>
    {
        public NoPagingSpecification(ISpecification<TX> src)
        {
            Criteria = src.Criteria;
            Includes = src.Includes;
            OrderBy = src.OrderBy;
            OrderByDescending = src.OrderByDescending;
            Skip = null;
            Take = null;
            IsPagingEnabled = false;
        }

        public Expression<Func<TX, bool>>? Criteria { get; }
        public List<Expression<Func<TX, object>>> Includes { get; }
        public Expression<Func<TX, object>>? OrderBy { get; }
        public Expression<Func<TX, object>>? OrderByDescending { get; }
        public int? Skip { get; }
        public int? Take { get; }
        public bool IsPagingEnabled { get; }
    }
}
