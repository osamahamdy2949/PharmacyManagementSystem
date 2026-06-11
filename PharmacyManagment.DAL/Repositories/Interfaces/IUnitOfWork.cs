using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    PharmacyDbContext Context { get; }
    IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, new();
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
