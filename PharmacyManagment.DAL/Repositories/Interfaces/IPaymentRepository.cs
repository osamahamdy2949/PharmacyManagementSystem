using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetHistoryAsync(int? customerId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetByCustomerAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetByInvoiceAsync(int invoiceId, CancellationToken ct = default);
    Task<Payment?> GetByIdWithCustomerAsync(int id, CancellationToken ct = default);
}
