using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class ShiftRepository : GenericRepository<Shift>, IShiftRepository
{
    public ShiftRepository(PharmacyDbContext context) : base(context)
    {
    }

    public Task<bool> HasActiveShiftAsync(string? userId, CancellationToken ct = default) =>
        Query().AsNoTracking().AnyAsync(s => s.UserId == userId && s.IsActive, ct);

    public Task<Shift?> GetActiveShiftAsync(string? userId, CancellationToken ct = default) =>
        Query().FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, ct);

    public Task<Shift?> GetActiveShiftByIdAsync(int shiftId, CancellationToken ct = default) =>
        Query().FirstOrDefaultAsync(s => s.Id == shiftId && s.IsActive, ct);

    public async Task<IReadOnlyList<Shift>> GetHistoryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = Query().AsNoTracking().AsQueryable();

        if (from.HasValue)
            query = query.Where(s => s.StartTime >= from.Value.Date);

        if (to.HasValue)
        {
            var endOfDay = to.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(s => s.StartTime <= endOfDay);
        }

        return await query.OrderByDescending(s => s.StartTime).ToListAsync(ct);
    }
}
