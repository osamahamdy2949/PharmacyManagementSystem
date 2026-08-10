using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class MedicineRepository : GenericRepository<Medicine>, IMedicineRepository
{
    public MedicineRepository(PharmacyDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Medicine>> GetAllWithCategoryAsync(string? search = null, CancellationToken ct = default)
    {
        var query = Query().AsNoTracking().Include(m => m.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(m =>
                m.TradeName.ToLower().Contains(term) ||
                m.ScientificName.ToLower().Contains(term));
        }

        return await query.OrderBy(m => m.TradeName).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Medicine>> SearchWithCategoryAsync(string term, int take, CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Include(m => m.Category)
            .Where(m =>
                m.SerialNumber.ToLower().Contains(term) ||
                m.TradeName.ToLower().Contains(term) ||
                m.ScientificName.ToLower().Contains(term) ||
                m.Manufacturer.ToLower().Contains(term))
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<Medicine?> GetByIdWithCategoryAsync(int id, CancellationToken ct = default) =>
        Query().AsNoTracking().Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<Medicine?> GetTrackedByIdAsync(int id, CancellationToken ct = default) =>
        Query().FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<bool> SerialNumberExistsForOtherMedicineAsync(string serialNumber, int medicineId, CancellationToken ct = default) =>
        Query().AsNoTracking().AnyAsync(m => m.SerialNumber == serialNumber && m.Id != medicineId, ct);

    public async Task<IReadOnlyDictionary<int, Medicine>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var medicineIds = ids.Distinct().ToList();
        if (medicineIds.Count == 0)
            return new Dictionary<int, Medicine>();

        return await Query()
            .AsNoTracking()
            .Where(m => medicineIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);
    }

    public async Task<IReadOnlyList<StockInfoData>> GetStockInfoAsync(IEnumerable<int> medicineIds, DateTime today, DateTime nearExpiry, CancellationToken ct = default)
    {
        var ids = medicineIds.Distinct().ToList();
        if (ids.Count == 0)
            return Array.Empty<StockInfoData>();

        return await Query()
            .SelectMany(m => m.MedicineBatches)
            .AsNoTracking()
            .Where(b =>
                ids.Contains(b.MedicineId) &&
                b.IsActive &&
                b.ExpiryDate > today &&
                b.CurrentQuantity > 0)
            .GroupBy(b => b.MedicineId)
            .Select(g => new StockInfoData
            {
                MedicineId = g.Key,
                QuantityInStock = g.Sum(b => b.CurrentQuantity),
                IsNearExpiry = g.Any(b => b.ExpiryDate <= nearExpiry)
            })
            .ToListAsync(ct);
    }
}
