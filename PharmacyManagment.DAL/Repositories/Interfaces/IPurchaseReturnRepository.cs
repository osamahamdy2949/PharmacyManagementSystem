using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IPurchaseReturnRepository : IGenericRepository<PurchaseReturn>
{
    Task<IReadOnlyList<PurchaseReturn>> GetAllWithSupplierAsync(CancellationToken ct = default);
    Task<PurchaseReturn?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<PurchaseInvoice?> GetInvoiceForCreateModelAsync(int invoiceId, CancellationToken ct = default);
    Task<PurchaseInvoice?> GetInvoiceWithItemsAsync(int invoiceId, CancellationToken ct = default);
    Task<int> CountAvailableBatchBoxesAsync(int medicineId, string batchNumber, CancellationToken ct = default);
}
