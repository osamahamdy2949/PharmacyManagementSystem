using PharmacyManagement.BLL.Common;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface IStockService
{
    Task<int> GetAvailableStockAsync(int medicineId, CancellationToken ct = default);
    Task<int> GetAvailableStockAsync(int medicineId, string dose, CancellationToken ct = default);
    Task<IReadOnlyList<MedicineBatch>> GetBatchesForSaleAsync(int medicineId, CancellationToken ct = default);
    Task<IReadOnlyList<MedicineBatch>> GetBatchesForSaleAsync(int medicineId, string dose, CancellationToken ct = default);
    Task<IReadOnlyList<BatchDeduction>> DeductStockFefoAsync(int medicineId, int quantity, int? referenceId = null, int? preferredBatchId = null, string? dose = null, CancellationToken ct = default);
    Task<IReadOnlyList<BatchDeduction>> DeductStockFromBatchAsync(int batchId, int quantity, int? referenceId = null, CancellationToken ct = default);
    Task<MedicineBatch> AddBatchStockAsync(MedicineBatch batch, int referenceId, bool recordTransaction = true, CancellationToken ct = default);
    Task RestoreToBatchAsync(int batchId, int quantity, int referenceId, CancellationToken ct = default);
    Task DeductFromBatchAsync(int batchId, int quantity, int referenceId, CancellationToken ct = default);
    Task RecordTransactionAsync(int medicineId, int quantity, StockTransactionType type, int? batchId, int? referenceId, string? notes = null, CancellationToken ct = default);
}
