using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class StockRepository : IStockRepository
{
    private readonly PharmacyDbContext _context;

    public StockRepository(PharmacyDbContext context) => _context = context;

    public Task<int> GetAvailableStockAsync(int medicineId, DateTime today, CancellationToken ct = default) =>
        ActiveSaleableBatches(today)
            .Where(b => b.MedicineId == medicineId)
            .SumAsync(b => b.CurrentQuantity, ct);

    public Task<int> GetAvailableStockAsync(int medicineId, string dose, DateTime today, CancellationToken ct = default) =>
        ActiveSaleableBatches(today)
            .Where(b => b.MedicineId == medicineId && b.Dose == dose)
            .SumAsync(b => b.CurrentQuantity, ct);

    public async Task<IReadOnlyList<MedicineBatch>> GetBatchesForSaleAsync(int medicineId, DateTime today, CancellationToken ct = default) =>
        await ActiveSaleableBatches(today)
            .Where(b => b.MedicineId == medicineId)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MedicineBatch>> GetBatchesForSaleAsync(int medicineId, string dose, DateTime today, CancellationToken ct = default) =>
        await ActiveSaleableBatches(today)
            .Where(b => b.MedicineId == medicineId && b.Dose == dose)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BatchReturnAvailabilityData>> GetReturnableBatchClonesAsync(int medicineId, string batchNumber, int take, CancellationToken ct = default) =>
        await _context.Set<MedicineBatch>()
            .Where(b => b.MedicineId == medicineId && b.BatchNumber == batchNumber && b.CurrentQuantity > 0)
            .Take(take)
            .Select(b => new BatchReturnAvailabilityData
            {
                BatchId = b.Id,
                MedicineId = b.MedicineId,
                BatchNumber = b.BatchNumber,
                CurrentQuantity = b.CurrentQuantity
            })
            .ToListAsync(ct);

    private IQueryable<MedicineBatch> ActiveSaleableBatches(DateTime today) =>
        _context.Set<MedicineBatch>().Where(b => b.IsActive && b.ExpiryDate > today && b.CurrentQuantity > 0);
}
