using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(PharmacyDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Payment>> GetHistoryAsync(int? customerId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var query = Query().AsNoTracking().Include(p => p.Customer).AsQueryable();

        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == customerId.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.PaymentDate >= fromDate.Value.Date);

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.PaymentDate <= endOfDay);
        }

        return await query.OrderByDescending(p => p.PaymentDate).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Payment>> GetByCustomerAsync(int customerId, CancellationToken ct = default) =>
        await Query()
            .AsNoTracking()
            .Include(p => p.Customer)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Payment>> GetByInvoiceAsync(int invoiceId, CancellationToken ct = default) =>
        await Query()
            .AsNoTracking()
            .Include(p => p.Customer)
            .Where(p => p.SalesInvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(ct);

    public Task<Payment?> GetByIdWithCustomerAsync(int id, CancellationToken ct = default) =>
        Query().AsNoTracking().Include(p => p.Customer).FirstOrDefaultAsync(p => p.Id == id, ct);
}
