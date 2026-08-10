using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface ISalesInvoiceRepository : IGenericRepository<SalesInvoice>
{
    Task<IReadOnlyList<SalesInvoice>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<SalesInvoice?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MedicineBatch>> SearchMedicineBatchesForSaleAsync(string? query, DateTime today, int take, CancellationToken ct = default);
    Task<IReadOnlyList<SalesInvoice>> GetUnpaidInvoicesByCustomerAsync(int customerId, CancellationToken ct = default);
    Task<ShiftTotalsData> GetShiftTotalsAsync(string userId, DateTime startTime, DateTime endTime, CancellationToken ct = default);
    Task<decimal> GetSalesTotalAsync(DateTime start, DateTime end, CancellationToken ct = default);
}
