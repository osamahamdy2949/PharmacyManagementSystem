using System.Collections.Concurrent;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class UnitOfWork : IUnitOfWork
{
    private readonly PharmacyDbContext _context;
    private readonly ConcurrentDictionary<string, object> _repositories = new();

    public UnitOfWork(PharmacyDbContext context) => _context = context;

    public PharmacyDbContext Context => _context;

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, new()
    {
        //check if the repository for the given entity type already exists in the context
        var typeName = typeof(TEntity).Name;

        //if it does, return it
        if (_repositories.TryGetValue(typeName, out var repository))
        {
            return (IGenericRepository<TEntity>)repository;
        }

        //otherwise create a new one and add it to the context and return it
        var newRepositoryInstance = new GenericRepository<TEntity>(_context);
        _repositories[typeName] = newRepositoryInstance;

        return newRepositoryInstance;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
