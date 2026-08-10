using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class ReportRepository : IReportRepository
{
    private readonly PharmacyDbContext _context;

    public ReportRepository(PharmacyDbContext context) => _context = context;

    public async Task<IReadOnlyList<MedicineBatch>> GetActiveInventoryBatchesAsync(CancellationToken ct = default) =>
        await _context.Set<MedicineBatch>()
            .AsNoTracking()
            .Include(b => b.Medicine).ThenInclude(m => m.Category)
            .Where(b => b.IsActive)
            .OrderBy(b => b.Medicine.TradeName)
            .ThenBy(b => b.ExpiryDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Medicine>> GetInventoryMedicinesAsync(DateTime today, CancellationToken ct = default) =>
        await _context.Set<Medicine>()
            .AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.MedicineBatches)
            .OrderBy(m => m.TradeName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Medicine>> GetLowStockMedicinesAsync(DateTime today, CancellationToken ct = default) =>
        await _context.Set<Medicine>()
            .AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.MedicineBatches)
            .Where(m => m.MedicineBatches.Sum(b => (b.ExpiryDate > today && b.CurrentQuantity > 0) ? b.CurrentQuantity : 0) < m.MinStockLevel)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Medicine>> GetExpiryMedicinesAsync(DateTime today, DateTime nearExpiry, CancellationToken ct = default) =>
        await _context.Set<Medicine>()
            .AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.MedicineBatches)
            .Where(m => m.MedicineBatches.Any(b => b.ExpiryDate <= nearExpiry && b.ExpiryDate >= today && b.CurrentQuantity > 0))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MedicineBatch>> GetExpiredBatchesAsync(DateTime today, CancellationToken ct = default) =>
        await _context.Set<MedicineBatch>()
            .AsNoTracking()
            .Include(b => b.Medicine)
            .Where(b => b.ExpiryDate < today && b.CurrentQuantity > 0)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SalesInvoice>> GetSalesReportInvoicesAsync(DateTime start, DateTime end, CancellationToken ct = default) =>
        await _context.Set<SalesInvoice>()
            .AsNoTracking()
            .Include(s => s.Customer)
            .Where(s => s.InvoiceDate >= start && s.InvoiceDate < end)
            .OrderByDescending(s => s.InvoiceDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseReportInvoicesAsync(DateTime start, DateTime end, CancellationToken ct = default) =>
        await _context.Set<PurchaseInvoice>()
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => p.InvoiceDate >= start && p.InvoiceDate < end)
            .OrderByDescending(p => p.InvoiceDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MedicineBatch>> GetPositiveQuantityBatchesWithMedicineAsync(CancellationToken ct = default) =>
        await _context.Set<MedicineBatch>()
            .Include(b => b.Medicine)
            .Where(b => b.CurrentQuantity > 0)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TopSellingMedicineData>> GetTopSellingMedicinesAsync(DateTime start, DateTime end, int take, CancellationToken ct = default) =>
        await _context.Set<SalesInvoiceItem>()
            .AsNoTracking()
            .Where(i => i.SalesInvoice.InvoiceDate >= start && i.SalesInvoice.InvoiceDate < end)
            .GroupBy(i => new { i.MedicineId, i.Medicine.TradeName })
            .Select(g => new TopSellingMedicineData
            {
                MedicineName = g.Key.TradeName,
                QuantitySold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SalesByCategoryData>> GetSalesByCategoryAsync(DateTime start, DateTime end, CancellationToken ct = default) =>
        await _context.Set<SalesInvoiceItem>()
            .AsNoTracking()
            .Where(i => i.SalesInvoice.InvoiceDate >= start && i.SalesInvoice.InvoiceDate < end)
            .GroupBy(i => i.Medicine.Category.Name)
            .Select(g => new SalesByCategoryData
            {
                CategoryName = g.Key,
                TotalSales = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.TotalSales)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<FinishedMedicineData>> GetFinishedMedicinesAsync(DateTime today, CancellationToken ct = default) =>
        await _context.Set<Medicine>()
            .AsNoTracking()
            .Where(m => !m.MedicineBatches.Any(b => b.ExpiryDate > today && b.CurrentQuantity > 0))
            .OrderBy(m => m.TradeName)
            .Select(m => new FinishedMedicineData
            {
                Id = m.Id,
                SerialNumber = m.SerialNumber,
                TradeName = m.TradeName,
                ScientificName = m.ScientificName,
                MedicineForm = m.MedicineForm.ToString(),
                CategoryName = m.Category.Name,
                Manufacturer = m.Manufacturer,
                PurchaseUnit = m.PurchaseUnit.ToString(),
                SaleUnit = m.SaleUnit.ToString(),
                UnitsPerPurchaseUnit = m.UnitsPerPurchaseUnit,
                PurchasePricePerPurchaseUnit = m.PurchasePrice
            })
            .ToListAsync(ct);
}
