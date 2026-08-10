using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IMedicineRepository : IGenericRepository<Medicine>
{
    Task<IReadOnlyList<Medicine>> GetAllWithCategoryAsync(string? search = null, CancellationToken ct = default);
    Task<IReadOnlyList<Medicine>> SearchWithCategoryAsync(string term, int take, CancellationToken ct = default);
    Task<Medicine?> GetByIdWithCategoryAsync(int id, CancellationToken ct = default);
    Task<Medicine?> GetTrackedByIdAsync(int id, CancellationToken ct = default);
    Task<bool> SerialNumberExistsForOtherMedicineAsync(string serialNumber, int medicineId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<int, Medicine>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<IReadOnlyList<StockInfoData>> GetStockInfoAsync(IEnumerable<int> medicineIds, DateTime today, DateTime nearExpiry, CancellationToken ct = default);
}
