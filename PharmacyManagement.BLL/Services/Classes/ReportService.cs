using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;
using PharmacyManagement.BLL.ViewModels.ReportViewModels;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly ISalesInvoiceRepository _salesInvoiceRepository;
    private readonly IPurchaseInvoiceRepository _purchaseInvoiceRepository;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ISalesReturnService _salesReturnService;
    private readonly INotificationService _notificationService;

    public ReportService(
        IReportRepository reportRepository,
        ISalesInvoiceRepository salesInvoiceRepository,
        IPurchaseInvoiceRepository purchaseInvoiceRepository,
        IDashboardRepository dashboardRepository,
        ISalesReturnService salesReturnService,
        INotificationService notificationService)
    {
        _reportRepository = reportRepository;
        _salesInvoiceRepository = salesInvoiceRepository;
        _purchaseInvoiceRepository = purchaseInvoiceRepository;
        _dashboardRepository = dashboardRepository;
        _salesReturnService = salesReturnService;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<BatchInventoryItemViewModel>> GetBatchInventoryAsync(InventoryFilter filter = InventoryFilter.All)
    {
        var today = DateTime.Today;
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);
        var batches = await _reportRepository.GetActiveInventoryBatchesAsync();

        var items = batches.Select(b =>
        {
            var medicine = b.Medicine;
            var unitsPerPurchase = Math.Max(1, b.UnitsPerPurchaseUnit > 0 ? b.UnitsPerPurchaseUnit : medicine.UnitsPerPurchaseUnit);
            var minLevel = b.MinStockLevel > 0 ? b.MinStockLevel : medicine.MinStockLevel;
            var status = GetInventoryStatus(b.CurrentQuantity, b.ExpiryDate, today, nearExpiry, minLevel);

            return new BatchInventoryItemViewModel
            {
                BatchId = b.Id,
                Sku = b.Sku,
                MedicineName = medicine.TradeName,
                MedicineForm = medicine.MedicineForm.ToString(),
                CategoryName = medicine.Category.Name,
                Dose = b.Dose,
                BatchNumber = b.BatchNumber,
                StockByPurchaseUnit = b.CurrentQuantity / unitsPerPurchase,
                StockBySellingUnit = b.CurrentQuantity,
                ExpiryDate = b.ExpiryDate,
                PurchasePrice = b.PurchasePrice,
                SellingPrice = b.SellingPrice,
                Status = status.Label,
                StatusColor = status.Color
            };
        }).ToList();

        return filter switch
        {
            InventoryFilter.LowStock => items.Where(i => i.Status == "Low Stock").ToList(),
            InventoryFilter.NearExpiry => items.Where(i => i.Status == "Near Expiry").ToList(),
            InventoryFilter.Expired => items.Where(i => i.Status == "Expired").ToList(),
            InventoryFilter.OutOfStock => items.Where(i => i.Status == "Out Of Stock").ToList(),
            _ => items
        };
    }

    public async Task<IReadOnlyList<InventoryReportItemViewModel>> GetInventoryReportAsync()
    {
        var today = DateTime.Today;
        var medicines = await _reportRepository.GetInventoryMedicinesAsync(today);

        return medicines.Select(m => new InventoryReportItemViewModel
        {
            Id = m.Id,
            SerialNumber = m.SerialNumber,
            TradeName = m.TradeName,
            CategoryName = m.Category.Name,
            QuantityInStock = m.MedicineBatches
                .Where(b => b.IsActive && b.ExpiryDate > today && b.CurrentQuantity > 0)
                .Sum(b => b.CurrentQuantity),
            SellingPrice = m.SellingPrice,
            PurchasePrice = m.PurchasePrice
        }).ToList();
    }

    public async Task<IReadOnlyList<MedicineViewModel>> GetLowStockReportAsync()
    {
        var today = DateTime.Today;
        var medicines = await _reportRepository.GetLowStockMedicinesAsync(today);

        return medicines.Select(m => new MedicineViewModel
        {
            Id = m.Id,
            SerialNumber = m.SerialNumber,
            TradeName = m.TradeName,
            ScientificName = m.ScientificName,
            Description = m.Description,
            MedicineForm = m.MedicineForm,
            PurchaseUnit = m.PurchaseUnit,
            SaleUnit = m.SaleUnit,
            UnitsPerPurchaseUnit = m.UnitsPerPurchaseUnit,
            PurchasePrice = m.PurchasePrice,
            SellingPrice = m.SellingPrice,
            QuantityInStock = m.MedicineBatches
                .Where(b => b.IsActive && b.ExpiryDate > today && b.CurrentQuantity > 0)
                .Sum(b => b.CurrentQuantity),
            Manufacturer = m.Manufacturer,
            Barcode = m.Barcode,
            StrengthValue = m.StrengthValue,
            StrengthUnit = m.StrengthUnit,
            MinStockLevel = m.MinStockLevel,
            IsActive = m.IsActive,
            CategoryId = m.CategoryId,
            CategoryName = m.Category.Name
        }).ToList();
    }

    public async Task<IReadOnlyList<MedicineViewModel>> GetExpiryReportAsync()
    {
        var today = DateTime.Today;
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);
        var medicines = await _reportRepository.GetExpiryMedicinesAsync(today, nearExpiry);

        return medicines.Select(m => new MedicineViewModel
        {
            Id = m.Id,
            SerialNumber = m.SerialNumber,
            TradeName = m.TradeName,
            ScientificName = m.ScientificName,
            Description = m.Description,
            MedicineForm = m.MedicineForm,
            PurchaseUnit = m.PurchaseUnit,
            SaleUnit = m.SaleUnit,
            UnitsPerPurchaseUnit = m.UnitsPerPurchaseUnit,
            PurchasePrice = m.PurchasePrice,
            SellingPrice = m.SellingPrice,
            QuantityInStock = m.MedicineBatches
                .Where(b => b.IsActive && b.ExpiryDate > today && b.CurrentQuantity > 0)
                .Sum(b => b.CurrentQuantity),
            Manufacturer = m.Manufacturer,
            Barcode = m.Barcode,
            StrengthValue = m.StrengthValue,
            StrengthUnit = m.StrengthUnit,
            MinStockLevel = m.MinStockLevel,
            IsActive = m.IsActive,
            CategoryId = m.CategoryId,
            CategoryName = m.Category.Name,
            IsNearExpiry = true
        }).ToList();
    }

    public async Task<IReadOnlyList<ExpiredMedicineReportItemViewModel>> GetExpiredMedicinesReportAsync()
    {
        var today = DateTime.Today;
        var batches = await _reportRepository.GetExpiredBatchesAsync(today);

        return batches.Select(b => new ExpiredMedicineReportItemViewModel
        {
            MedicineName = b.Medicine.TradeName,
            BatchNumber = b.BatchNumber,
            ExpiryDate = b.ExpiryDate,
            Quantity = b.CurrentQuantity,
            LossValue = b.CurrentQuantity * b.PurchasePrice / Math.Max(1, b.Medicine.UnitsPerPurchaseUnit)
        }).ToList();
    }

    public async Task<IReadOnlyList<SalesReportItemViewModel>> GetSalesReportAsync(
        ReportPeriod period = ReportPeriod.Monthly,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (start, end) = GetDateRange(period, from, to);
        var invoices = await _reportRepository.GetSalesReportInvoicesAsync(start, end);

        return invoices.Select(s => new SalesReportItemViewModel
        {
            InvoiceId = s.Id,
            InvoiceDate = s.InvoiceDate,
            CustomerName = s.Customer.Name,
            SaleType = s.SaleType.ToString(),
            TotalAmount = s.TotalAmount
        }).ToList();
    }

    public async Task<IReadOnlyList<PurchaseReportItemViewModel>> GetPurchaseReportAsync(
        ReportPeriod period = ReportPeriod.Monthly,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (start, end) = GetDateRange(period, from, to);
        var invoices = await _reportRepository.GetPurchaseReportInvoicesAsync(start, end);

        return invoices.Select(p => new PurchaseReportItemViewModel
        {
            InvoiceId = p.Id,
            InvoiceDate = p.InvoiceDate,
            SupplierName = p.Supplier.Name,
            TotalAmount = p.TotalAmount
        }).ToList();
    }

    public async Task<ProfitReportViewModel> GetProfitReportAsync(
        ReportPeriod period = ReportPeriod.Monthly,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (start, end) = GetDateRange(period, from, to);
        var sales = await _salesInvoiceRepository.GetSalesTotalAsync(start, end);
        var purchases = await _purchaseInvoiceRepository.GetPurchasesTotalAsync(start, end);

        return new ProfitReportViewModel
        {
            TotalSales = sales,
            TotalPurchases = purchases,
            Profit = sales - purchases,
            FromDate = start,
            ToDate = end.AddDays(-1)
        };
    }

    public async Task<IReadOnlyList<TopSellingMedicineViewModel>> GetTopSellingMedicinesReportAsync(
        ReportPeriod period = ReportPeriod.Monthly,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (start, end) = GetDateRange(period, from, to);
        var items = await _reportRepository.GetTopSellingMedicinesAsync(start, end, 20);

        return items.Select(i => new TopSellingMedicineViewModel
        {
            MedicineName = i.MedicineName,
            QuantitySold = i.QuantitySold
        }).ToList();
    }

    public async Task<IReadOnlyList<SalesByCategoryViewModel>> GetSalesByCategoryReportAsync(
        ReportPeriod period = ReportPeriod.Monthly,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (start, end) = GetDateRange(period, from, to);
        var items = await _reportRepository.GetSalesByCategoryAsync(start, end);

        return items.Select(i => new SalesByCategoryViewModel
        {
            CategoryName = i.CategoryName,
            TotalSales = i.TotalSales
        }).ToList();
    }

    public async Task<IReadOnlyList<FinishedMedicineReportItemViewModel>> GetFinishedMedicinesReportAsync()
    {
        var today = DateTime.Today;
        var items = await _reportRepository.GetFinishedMedicinesAsync(today);

        return items.Select(m => new FinishedMedicineReportItemViewModel
        {
            Id = m.Id,
            SerialNumber = m.SerialNumber,
            TradeName = m.TradeName,
            ScientificName = m.ScientificName,
            MedicineForm = m.MedicineForm,
            CategoryName = m.CategoryName,
            Manufacturer = m.Manufacturer,
            PurchaseUnit = m.PurchaseUnit,
            SaleUnit = m.SaleUnit,
            UnitsPerPurchaseUnit = m.UnitsPerPurchaseUnit,
            PurchasePricePerPurchaseUnit = m.PurchasePricePerPurchaseUnit
        }).ToList();
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);
        var todayUtc = DateTime.UtcNow.Date;
        var data = await _dashboardRepository.GetMainDashboardDataAsync(today, tomorrow, monthStart, nearExpiry, todayUtc);

        return new DashboardViewModel
        {
            TotalMedicines = data.TotalMedicines,
            TotalCategories = data.TotalCategories,
            TotalSuppliers = data.TotalSuppliers,
            TotalCustomers = data.TotalCustomers,
            LowStockCount = data.LowStockCount,
            CustomersToday = data.CustomersToday,
            ActiveUsers = data.ActiveUsers,
            UnreadNotifications = await _notificationService.GetUnreadCountAsync(),
            ExpiredCount = data.ExpiredCount,
            NearExpiryCount = data.NearExpiryCount,
            TodaySales = data.TodaySales,
            MonthlySales = data.MonthlySales,
            TodayPurchases = data.TodayPurchases,
            MonthlyPurchases = data.MonthlyPurchases,
            TodayProfit = data.TodaySales - data.TodayPurchases,
            MonthlyProfit = data.MonthlySales - data.MonthlyPurchases,
            SalesReturnCount = await _salesReturnService.GetReturnCountAsync(),
            InventoryTotalValue = data.InventoryTotalValue
        };
    }

    private static (DateTime Start, DateTime End) GetDateRange(ReportPeriod period, DateTime? from, DateTime? to)
    {
        var today = DateTime.Today;
        return period switch
        {
            ReportPeriod.Daily => (today, today.AddDays(1)),
            ReportPeriod.Weekly => (today.AddDays(-6), today.AddDays(1)),
            ReportPeriod.Monthly => (new DateTime(today.Year, today.Month, 1), today.AddDays(1)),
            ReportPeriod.Yearly => (new DateTime(today.Year, 1, 1), today.AddDays(1)),
            ReportPeriod.Custom when from.HasValue && to.HasValue => (from.Value.Date, to.Value.Date.AddDays(1)),
            _ => (new DateTime(today.Year, today.Month, 1), today.AddDays(1))
        };
    }

    private static (string Label, string Color) GetInventoryStatus(
        int stock, DateTime expiry, DateTime today, DateTime nearExpiry, int threshold)
    {
        if (stock == 0) return ("Out Of Stock", "orange");
        if (expiry < today) return ("Expired", "red");
        if (expiry <= nearExpiry) return ("Near Expiry", "red");
        if (stock < threshold) return ("Low Stock", "yellow");
        return ("In Stock", "green");
    }
}
