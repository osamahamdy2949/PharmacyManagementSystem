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

    public async Task<IEnumerable<TEntity>> GetAllAsync(bool tracking, CancellationToken ct)
    {
        IQueryable<TEntity> query = tracking ? _dbSet : _dbSet.AsNoTracking();

        return await query.ToListAsync(ct);
    }

    public async Task<TEntity?> GetByIdAsync(int id, bool tracking, CancellationToken ct)
    {
        //return await _dbSet.FindAsync(id, ct); FindAsync Use Tracking By Defualt 

        if (tracking)
            return await _dbSet.FindAsync(id, ct);

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }
    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
    {
        return _dbSet.AsNoTracking().AnyAsync(predicate, ct);
    }
    public void Add(TEntity entity) => _dbSet.AddAsync(entity);

    public void Update(TEntity entity) => _dbSet.Update(entity);

    public void Remove(TEntity entity) => _dbSet.Remove(entity);
    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool tracking, CancellationToken ct)
    {
        IQueryable<TEntity> query = tracking ? _dbSet : _dbSet.AsNoTracking();

        return await query.FirstOrDefaultAsync(predicate, ct);
    }
    public IQueryable<TEntity> Query() => _dbSet.AsQueryable();

    //private static IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, Expression<Func<TEntity, object>>[] includes)
    //{
    //    foreach (var include in includes)
    //        query = query.Include(include);
    //    return query;
    //}
}
