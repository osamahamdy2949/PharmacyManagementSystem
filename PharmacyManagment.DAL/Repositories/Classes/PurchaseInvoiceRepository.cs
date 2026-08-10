using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class PurchaseInvoiceRepository : GenericRepository<PurchaseInvoice>, IPurchaseInvoiceRepository
{
    public PurchaseInvoiceRepository(PharmacyDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PurchaseInvoice>> GetAllWithDetailsAsync(CancellationToken ct = default) =>
        await Query()
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .OrderByDescending(p => p.InvoiceDate)
            .ToListAsync(ct);

    public Task<PurchaseInvoice?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        Query()
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<PurchaseInvoice?> GetByIdWithItemsAsync(int id, CancellationToken ct = default) =>
        Query()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<PurchaseInvoice?> GetByIdWithItemsAndBatchesAsync(int id, CancellationToken ct = default) =>
        Query()
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .Include(p => p.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<decimal> GetPurchasesTotalAsync(DateTime start, DateTime end, CancellationToken ct = default) =>
        Query().AsNoTracking().Where(p => p.InvoiceDate >= start && p.InvoiceDate < end).SumAsync(p => p.TotalAmount, ct);
}
