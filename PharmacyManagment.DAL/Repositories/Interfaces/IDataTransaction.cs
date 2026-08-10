namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IDataTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
