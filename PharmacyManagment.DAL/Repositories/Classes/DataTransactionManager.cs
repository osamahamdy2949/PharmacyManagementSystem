using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class DataTransactionManager : IDataTransactionManager
{
    private readonly PharmacyDbContext _context;

    public DataTransactionManager(PharmacyDbContext context) => _context = context;

    public async Task<IDataTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        new EfDataTransaction(await _context.Database.BeginTransactionAsync(ct));

    public bool IsConcurrencyException(Exception exception) =>
        exception is DbUpdateConcurrencyException;

    private sealed class EfDataTransaction : IDataTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfDataTransaction(IDbContextTransaction transaction) => _transaction = transaction;

        public Task CommitAsync(CancellationToken ct = default) => _transaction.CommitAsync(ct);

        public Task RollbackAsync(CancellationToken ct = default) => _transaction.RollbackAsync(ct);

        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
