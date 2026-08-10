using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<int> GetUnreadCountAsync(string? userId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetRecentAsync(int count, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetLowStockMedicineNamesAsync(DateTime today, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetNearExpiryMedicineNamesAsync(DateTime today, DateTime nearExpiry, CancellationToken ct = default);
}
