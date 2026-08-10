using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    private readonly PharmacyDbContext _context;

    public NotificationRepository(PharmacyDbContext context) : base(context) => _context = context;

    public Task<int> GetUnreadCountAsync(string? userId = null, CancellationToken ct = default) =>
        Query().CountAsync(n => !n.IsRead && (userId == null || n.UserId == null || n.UserId == userId), ct);

    public async Task<IReadOnlyList<Notification>> GetRecentAsync(int count, CancellationToken ct = default) =>
        await Query()
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetLowStockMedicineNamesAsync(DateTime today, CancellationToken ct = default) =>
        await _context.Set<Medicine>()
            .Where(m => m.MedicineBatches.Sum(b => (b.ExpiryDate > today && b.CurrentQuantity > 0) ? b.CurrentQuantity : 0) < m.MinStockLevel)
            .Select(m => m.TradeName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetNearExpiryMedicineNamesAsync(DateTime today, DateTime nearExpiry, CancellationToken ct = default) =>
        await _context.Set<MedicineBatch>()
            .Include(b => b.Medicine)
            .Where(b => b.ExpiryDate >= today && b.ExpiryDate <= nearExpiry && b.CurrentQuantity > 0)
            .Select(b => b.Medicine.TradeName)
            .Distinct()
            .ToListAsync(ct);
}
