using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IMedicineBatchRepository : IGenericRepository<MedicineBatch>
{
    Task<IReadOnlyList<string>> GetBatchNumbersAsync(bool distinct = false, CancellationToken ct = default);
    Task<IReadOnlyList<PendingBatchData>> GetPendingBatchesAsync(CancellationToken ct = default);
    Task<MedicineBatch?> GetInactiveWithMedicineAsync(int batchId, CancellationToken ct = default);
    Task<IReadOnlyList<string?>> GetExistingBarcodesAsync(IEnumerable<string> barcodes, CancellationToken ct = default);
    Task<int> GetPurchaseInvoiceIdByBatchIdAsync(int batchId, CancellationToken ct = default);
    Task<MedicineBatch?> GetTrackedInactiveByIdAsync(int batchId, CancellationToken ct = default);
}
