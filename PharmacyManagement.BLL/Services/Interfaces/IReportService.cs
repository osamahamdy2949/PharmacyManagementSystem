using PharmacyManagement.BLL.ViewModels;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;
using PharmacyManagement.BLL.ViewModels.ReportViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<BatchInventoryItemViewModel>> GetBatchInventoryAsync(InventoryFilter filter = InventoryFilter.All);
    Task<IReadOnlyList<InventoryReportItemViewModel>> GetInventoryReportAsync();
    Task<IReadOnlyList<MedicineViewModel>> GetLowStockReportAsync();
    Task<IReadOnlyList<MedicineViewModel>> GetExpiryReportAsync();
    Task<IReadOnlyList<ExpiredMedicineReportItemViewModel>> GetExpiredMedicinesReportAsync();
    Task<IReadOnlyList<SalesReportItemViewModel>> GetSalesReportAsync(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null);
    Task<IReadOnlyList<PurchaseReportItemViewModel>> GetPurchaseReportAsync(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null);
    Task<ProfitReportViewModel> GetProfitReportAsync(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null);
    Task<IReadOnlyList<TopSellingMedicineViewModel>> GetTopSellingMedicinesReportAsync(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null);
    Task<IReadOnlyList<SalesByCategoryViewModel>> GetSalesByCategoryReportAsync(ReportPeriod period = ReportPeriod.Monthly, DateTime? from = null, DateTime? to = null);
    Task<IReadOnlyList<FinishedMedicineReportItemViewModel>> GetFinishedMedicinesReportAsync();
    Task<DashboardViewModel> GetDashboardAsync();
}
