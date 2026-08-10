namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IDataTransactionManager
{
    Task<IDataTransaction> BeginTransactionAsync(CancellationToken ct = default);
    bool IsConcurrencyException(Exception exception);
}
