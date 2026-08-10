using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IShiftRepository : IGenericRepository<Shift>
{
    Task<bool> HasActiveShiftAsync(string? userId, CancellationToken ct = default);
    Task<Shift?> GetActiveShiftAsync(string? userId, CancellationToken ct = default);
    Task<Shift?> GetActiveShiftByIdAsync(int shiftId, CancellationToken ct = default);
    Task<IReadOnlyList<Shift>> GetHistoryAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
}
