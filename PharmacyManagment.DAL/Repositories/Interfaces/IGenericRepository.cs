using PharmacyManagement.DAL.Data.Entities;
using System.Linq.Expressions;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : BaseEntity , new()
{
    Task<IEnumerable<TEntity>> GetAllAsync(bool tracking = false, CancellationToken ct = default);
    Task<TEntity?> GetByIdAsync(int id, bool tracking = false, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool tracking = false, CancellationToken ct = default);
    IQueryable<TEntity> Query();
}
