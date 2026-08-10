using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class SalesReturnRepository : GenericRepository<SalesReturn>, ISalesReturnRepository
{
    private readonly PharmacyDbContext _context;

    public SalesReturnRepository(PharmacyDbContext context) : base(context) => _context = context;

    public async Task<IReadOnlyList<SalesReturn>> GetAllWithCustomerAsync(CancellationToken ct = default) =>
        await Query()
            .Include(r => r.Customer)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync(ct);

    public Task<SalesReturn?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        Query()
            .Include(r => r.Customer)
            .Include(r => r.Items).ThenInclude(i => i.Medicine)
            .Include(r => r.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<SalesInvoice?> GetInvoiceForCreateModelAsync(int invoiceId, CancellationToken ct = default) =>
        _context.Set<SalesInvoice>()
            .Include(s => s.Items).ThenInclude(i => i.Medicine)
            .Include(s => s.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(s => s.Id == invoiceId, ct);

    public Task<SalesInvoice?> GetInvoiceWithItemsAsync(int invoiceId, CancellationToken ct = default) =>
        _context.Set<SalesInvoice>()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == invoiceId, ct);

    public Task<int> GetReturnedQuantityAsync(int salesInvoiceId, int medicineBatchId, CancellationToken ct = default) =>
        _context.Set<SalesReturnItem>()
            .Where(ri => ri.SalesReturn.SalesInvoiceId == salesInvoiceId && ri.MedicineBatchId == medicineBatchId)
            .SumAsync(ri => ri.Quantity, ct);

    public Task<int> GetReturnCountAsync(CancellationToken ct = default) =>
        Query().CountAsync(ct);
}
