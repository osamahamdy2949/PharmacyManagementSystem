namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IApplicationUserRepository
{
    Task<int> CountAsync(CancellationToken ct = default);
    Task<string?> GetFullNameByIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, string>> GetFullNamesByIdsAsync(IEnumerable<string> userIds, CancellationToken ct = default);
}
