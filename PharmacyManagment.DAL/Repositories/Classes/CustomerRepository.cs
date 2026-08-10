using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Data.Entities.Enums;
using PharmacyManagement.DAL.Repositories.Interfaces;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    private readonly PharmacyDbContext _context;

    public CustomerRepository(PharmacyDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<CustomerProfileData?> GetCustomerProfileAsync(int id, CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CustomerProfileData
            {
                Customer = c,
                TotalInvoices = c.SalesInvoices.Count
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<CustomerDebtHistoryData>> GetCustomerDebtHistoryAsync(int id, CancellationToken ct = default)
    {
        return await _context.Set<SalesInvoice>()
            .AsNoTracking()
            .Where(s => s.CustomerId == id && s.SaleType == SaleType.Credit)
            .OrderByDescending(s => s.InvoiceDate)
            .Select(s => new CustomerDebtHistoryData
            {
                InvoiceId = s.Id,
                InvoiceDate = s.InvoiceDate,
                TotalAmount = s.TotalAmount,
                PaidAmount = s.PaidAmount,
                RemainingAmount = s.RemainingAmount,
                PaymentStatus = s.PaymentStatus.ToString()
            })
            .ToListAsync(ct);
    }
}
