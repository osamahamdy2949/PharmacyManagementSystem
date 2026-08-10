using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class MedicineBatchRepository : GenericRepository<MedicineBatch>, IMedicineBatchRepository
{
    private readonly PharmacyDbContext _context;

    public MedicineBatchRepository(PharmacyDbContext context) : base(context) => _context = context;

    public async Task<IReadOnlyList<string>> GetBatchNumbersAsync(bool distinct = false, CancellationToken ct = default)
    {
        var query = Query().AsNoTracking().Select(b => b.BatchNumber);

        if (distinct)
            query = query.Distinct().OrderBy(b => b);

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PendingBatchData>> GetPendingBatchesAsync(CancellationToken ct = default)
    {
        var pendingBatches = _context.Set<MedicineBatch>()
            .AsNoTracking()
            .Where(b => !b.IsActive && b.CurrentQuantity > 0);

        var invoiceItems = _context.Set<PurchaseInvoiceItem>().AsNoTracking();

        return await (
            from batch in pendingBatches
            join item in invoiceItems on batch.Id equals item.MedicineBatchId into batchItems
            from invoiceItem in batchItems.Take(1).DefaultIfEmpty()
            orderby batch.Medicine.TradeName
            select new PendingBatchData
            {
                BatchId = batch.Id,
                PurchaseInvoiceId = invoiceItem == null ? 0 : invoiceItem.PurchaseInvoiceId,
                MedicineName = batch.Medicine.TradeName,
                Sku = batch.Sku,
                Dose = batch.Dose,
                BatchNumber = batch.BatchNumber,
                SupplierName = invoiceItem == null ? "" : invoiceItem.PurchaseInvoice.Supplier.Name,
                PendingQuantity = batch.CurrentQuantity,
                PurchaseUnit = batch.Medicine.PurchaseUnit.ToString()
            })
            .ToListAsync(ct);
    }

    public Task<MedicineBatch?> GetInactiveWithMedicineAsync(int batchId, CancellationToken ct = default) =>
        Query().Include(b => b.Medicine).FirstOrDefaultAsync(b => b.Id == batchId && !b.IsActive, ct);

    public async Task<IReadOnlyList<string?>> GetExistingBarcodesAsync(IEnumerable<string> barcodes, CancellationToken ct = default)
    {
        var barcodeList = barcodes.ToList();
        if (barcodeList.Count == 0)
            return Array.Empty<string?>();

        return await Query()
            .Where(b => b.Barcode != null && barcodeList.Contains(b.Barcode))
            .Select(b => b.Barcode)
            .ToListAsync(ct);
    }

    public Task<int> GetPurchaseInvoiceIdByBatchIdAsync(int batchId, CancellationToken ct = default) =>
        _context.Set<PurchaseInvoiceItem>()
            .Where(i => i.MedicineBatchId == batchId)
            .Select(i => i.PurchaseInvoiceId)
            .FirstOrDefaultAsync(ct);

    public Task<MedicineBatch?> GetTrackedInactiveByIdAsync(int batchId, CancellationToken ct = default) =>
        Query().FirstOrDefaultAsync(b => b.Id == batchId && !b.IsActive, ct);
}
