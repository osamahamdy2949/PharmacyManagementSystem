using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class PurchaseReturnRepository : GenericRepository<PurchaseReturn>, IPurchaseReturnRepository
{
    private readonly PharmacyDbContext _context;

    public PurchaseReturnRepository(PharmacyDbContext context) : base(context) => _context = context;

    public async Task<IReadOnlyList<PurchaseReturn>> GetAllWithSupplierAsync(CancellationToken ct = default) =>
        await Query()
            .Include(r => r.Supplier)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync(ct);

    public Task<PurchaseReturn?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default) =>
        Query()
            .Include(r => r.Supplier)
            .Include(r => r.Items).ThenInclude(i => i.Medicine)
            .Include(r => r.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<PurchaseInvoice?> GetInvoiceForCreateModelAsync(int invoiceId, CancellationToken ct = default) =>
        _context.Set<PurchaseInvoice>()
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .Include(p => p.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(p => p.Id == invoiceId, ct);

    public Task<PurchaseInvoice?> GetInvoiceWithItemsAsync(int invoiceId, CancellationToken ct = default) =>
        _context.Set<PurchaseInvoice>()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == invoiceId, ct);

    public Task<int> CountAvailableBatchBoxesAsync(int medicineId, string batchNumber, CancellationToken ct = default) =>
        _context.Set<MedicineBatch>()
            .Where(b => b.MedicineId == medicineId && b.BatchNumber == batchNumber && b.CurrentQuantity > 0)
            .CountAsync(ct);
}
