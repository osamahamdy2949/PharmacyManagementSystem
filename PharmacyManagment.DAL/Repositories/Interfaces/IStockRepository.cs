using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IStockRepository
{
    Task<int> GetAvailableStockAsync(int medicineId, DateTime today, CancellationToken ct = default);
    Task<int> GetAvailableStockAsync(int medicineId, string dose, DateTime today, CancellationToken ct = default);
    Task<IReadOnlyList<MedicineBatch>> GetBatchesForSaleAsync(int medicineId, DateTime today, CancellationToken ct = default);
    Task<IReadOnlyList<MedicineBatch>> GetBatchesForSaleAsync(int medicineId, string dose, DateTime today, CancellationToken ct = default);
    Task<IReadOnlyList<BatchReturnAvailabilityData>> GetReturnableBatchClonesAsync(int medicineId, string batchNumber, int take, CancellationToken ct = default);
}
