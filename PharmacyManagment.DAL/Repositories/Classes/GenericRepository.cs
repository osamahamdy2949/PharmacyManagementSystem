using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity, new()
{
    private readonly PharmacyDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(PharmacyDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null, bool tracking = false, CancellationToken ct = default)
    {
        IQueryable<TEntity> query = tracking ? _dbSet : _dbSet.AsNoTracking();

        if (predicate is not null) query = query.Where(predicate);

        return await query.ToListAsync(ct);
    }

    public async Task<TEntity?> GetByIdAsync(int id, bool tracking = false, CancellationToken ct = default)
    {
        IQueryable<TEntity> query = tracking ? _dbSet : _dbSet.AsNoTracking();

        return await query.FirstOrDefaultAsync(entity => entity.Id == id, ct);
    }
    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
    {
        return _dbSet.AsNoTracking().AnyAsync(predicate, ct);
    }
    public void Add(TEntity entity) => _dbSet.AddAsync(entity);

    public void Update(TEntity entity) => _dbSet.Update(entity);

    public void Delete(TEntity entity) => _dbSet.Remove(entity);
    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool tracking, CancellationToken ct)
    {
        IQueryable<TEntity> query = tracking ? _dbSet : _dbSet.AsNoTracking();

        return await query.FirstOrDefaultAsync(predicate, ct);
    }
    public IQueryable<TEntity> Query() => _dbSet.AsQueryable();

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (predicate is not null) query = query.Where(predicate);

        return await query.CountAsync(ct);
    }
}
