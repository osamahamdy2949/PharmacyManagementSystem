using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class ApplicationUserRepository : IApplicationUserRepository
{
    private readonly PharmacyDbContext _context;

    public ApplicationUserRepository(PharmacyDbContext context) => _context = context;

    public Task<int> CountAsync(CancellationToken ct = default) =>
        _context.Set<ApplicationUser>().AsNoTracking().CountAsync(ct);

    public Task<string?> GetFullNameByIdAsync(string userId, CancellationToken ct = default) =>
        _context.Set<ApplicationUser>()
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyDictionary<string, string>> GetFullNamesByIdsAsync(IEnumerable<string> userIds, CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<string, string>();

        return await _context.Set<ApplicationUser>()
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);
    }
}
