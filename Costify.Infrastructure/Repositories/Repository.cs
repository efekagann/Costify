using System.Linq.Expressions;
using Costify.Domain.Entities.Base;
using Costify.Domain.Interfaces.Repositories;
using Costify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Costify.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly CostifyDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(CostifyDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _dbSet.AnyAsync(e => e.Id == id);

    public IQueryable<T> Query() => _dbSet.AsQueryable();
}
