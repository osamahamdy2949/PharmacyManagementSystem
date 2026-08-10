using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Data.Entities.Enums;
using PharmacyManagement.DAL.Repositories.Interfaces;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class SalesInvoiceRepository : GenericRepository<SalesInvoice>, ISalesInvoiceRepository
{
    private readonly PharmacyDbContext _context;

    public SalesInvoiceRepository(PharmacyDbContext context) : base(context) => _context = context;

    public async Task<IReadOnlyList<SalesInvoice>> GetAllWithDetailsAsync(CancellationToken ct = default) =>
        await Query()
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Items).ThenInclude(i => i.Medicine)
            .Include(s => s.Items).ThenInclude(i => i.MedicineBatch)
            .OrderByDescending(s => s.InvoiceDate)
            .ToListAsync(ct);

    public Task<SalesInvoice?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        Query()
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Items).ThenInclude(i => i.Medicine)
            .Include(s => s.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<MedicineBatch>> SearchMedicineBatchesForSaleAsync(string? query, DateTime today, int take, CancellationToken ct = default)
    {
        var batchQuery = _context.Set<MedicineBatch>()
            .AsNoTracking()
            .Include(b => b.Medicine)
            .Where(b => b.IsActive && b.ExpiryDate > today && b.CurrentQuantity > 0);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim().ToUpper();
            batchQuery = batchQuery.Where(b =>
                b.Medicine.TradeName.ToUpper().Contains(term) ||
                b.Medicine.ScientificName.ToUpper().Contains(term) ||
                b.Dose.ToUpper().Contains(term) ||
                (b.Barcode != null && b.Barcode.ToUpper() == term));
        }

        return await batchQuery
            .OrderBy(b => b.Medicine.TradeName)
            .ThenBy(b => b.Dose)
            .ThenBy(b => b.ExpiryDate)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SalesInvoice>> GetUnpaidInvoicesByCustomerAsync(int customerId, CancellationToken ct = default) =>
        await Query()
            .Where(s => s.CustomerId == customerId && s.RemainingAmount > 0)
            .OrderBy(s => s.InvoiceDate)
            .ToListAsync(ct);

    public async Task<ShiftTotalsData> GetShiftTotalsAsync(string userId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        var salesQuery = Query()
            .AsNoTracking()
            .Where(s => s.CreatedByUserId == userId && s.CreatedAt >= startTime && s.CreatedAt <= endTime);

        var cashSales = await salesQuery
            .Where(s => s.SaleType == SaleType.Cash)
            .SumAsync(s => s.TotalAmount, ct);

        var creditSales = await salesQuery
            .Where(s => s.SaleType == SaleType.Credit)
            .SumAsync(s => s.TotalAmount, ct);

        var creditSaleIds = await salesQuery
            .Where(s => s.SaleType == SaleType.Credit)
            .Select(s => s.Id)
            .ToListAsync(ct);

        var creditInvoicePayments = creditSaleIds.Count == 0
            ? 0
            : await _context.Set<Payment>()
                .AsNoTracking()
                .Where(p => p.SalesInvoiceId.HasValue
                    && creditSaleIds.Contains(p.SalesInvoiceId.Value)
                    && p.CreatedAt >= startTime
                    && p.CreatedAt <= endTime)
                .SumAsync(p => p.AmountPaid, ct);

        var creditInvoicePaidAmount = await salesQuery
            .Where(s => s.SaleType == SaleType.Credit)
            .SumAsync(s => s.PaidAmount, ct);

        var initialCreditPayments = Math.Max(0, creditInvoicePaidAmount - creditInvoicePayments);

        var payments = await _context.Set<Payment>()
            .AsNoTracking()
            .Where(p => p.RecordedByUserId == userId && p.CreatedAt >= startTime && p.CreatedAt <= endTime)
            .SumAsync(p => p.AmountPaid, ct);

        var purchases = await _context.Set<PurchaseInvoice>()
            .AsNoTracking()
            .Where(p => p.CreatedByUserId == userId && p.CreatedAt >= startTime && p.CreatedAt <= endTime)
            .SumAsync(p => p.TotalAmount, ct);

        return new ShiftTotalsData
        {
            CashSales = cashSales,
            CreditSales = creditSales,
            TotalPurchases = purchases,
            TotalPaymentsReceived = payments + initialCreditPayments
        };
    }

    public Task<decimal> GetSalesTotalAsync(DateTime start, DateTime end, CancellationToken ct = default) =>
        Query().AsNoTracking().Where(s => s.InvoiceDate >= start && s.InvoiceDate < end).SumAsync(s => s.TotalAmount, ct);
}
