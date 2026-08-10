using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IReportRepository
{
    Task<IReadOnlyList<MedicineBatch>> GetActiveInventoryBatchesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Medicine>> GetInventoryMedicinesAsync(DateTime today, CancellationToken ct = default);
    Task<IReadOnlyList<Medicine>> GetLowStockMedicinesAsync(DateTime today, CancellationToken ct = default);
    Task<IReadOnlyList<Medicine>> GetExpiryMedicinesAsync(DateTime today, DateTime nearExpiry, CancellationToken ct = default);
    Task<IReadOnlyList<MedicineBatch>> GetExpiredBatchesAsync(DateTime today, CancellationToken ct = default);
    Task<IReadOnlyList<SalesInvoice>> GetSalesReportInvoicesAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseReportInvoicesAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<IReadOnlyList<MedicineBatch>> GetPositiveQuantityBatchesWithMedicineAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TopSellingMedicineData>> GetTopSellingMedicinesAsync(DateTime start, DateTime end, int take, CancellationToken ct = default);
    Task<IReadOnlyList<SalesByCategoryData>> GetSalesByCategoryAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<IReadOnlyList<FinishedMedicineData>> GetFinishedMedicinesAsync(DateTime today, CancellationToken ct = default);
}
