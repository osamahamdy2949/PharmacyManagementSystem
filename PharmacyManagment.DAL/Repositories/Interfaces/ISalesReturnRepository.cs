using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface ISalesReturnRepository : IGenericRepository<SalesReturn>
{
    Task<IReadOnlyList<SalesReturn>> GetAllWithCustomerAsync(CancellationToken ct = default);
    Task<SalesReturn?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<SalesInvoice?> GetInvoiceForCreateModelAsync(int invoiceId, CancellationToken ct = default);
    Task<SalesInvoice?> GetInvoiceWithItemsAsync(int invoiceId, CancellationToken ct = default);
    Task<int> GetReturnedQuantityAsync(int salesInvoiceId, int medicineBatchId, CancellationToken ct = default);
    Task<int> GetReturnCountAsync(CancellationToken ct = default);
}
