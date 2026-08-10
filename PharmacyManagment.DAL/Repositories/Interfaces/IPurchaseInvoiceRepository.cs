using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IPurchaseInvoiceRepository : IGenericRepository<PurchaseInvoice>
{
    Task<IReadOnlyList<PurchaseInvoice>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<PurchaseInvoice?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<PurchaseInvoice?> GetByIdWithItemsAsync(int id, CancellationToken ct = default);
    Task<PurchaseInvoice?> GetByIdWithItemsAndBatchesAsync(int id, CancellationToken ct = default);
    Task<decimal> GetPurchasesTotalAsync(DateTime start, DateTime end, CancellationToken ct = default);
}
