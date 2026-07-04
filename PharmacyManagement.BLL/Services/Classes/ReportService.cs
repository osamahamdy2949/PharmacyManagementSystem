using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;
using PharmacyManagement.BLL.ViewModels.ReportViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISalesReturnService _salesReturnService;
    private readonly INotificationService _notificationService;

    public ReportService(
        IUnitOfWork unitOfWork,
        ISalesReturnService salesReturnService,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _salesReturnService = salesReturnService;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<BatchInventoryItemViewModel>> GetBatchInventoryAsync(InventoryFilter filter = InventoryFilter.All)
    {
        var today = DateTime.Today;
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);
        var batches = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .AsNoTracking()
            .Include(b => b.Medicine).ThenInclude(m => m.Category)
            .Where(b => b.IsActive)
            .OrderBy(b => b.Medicine.TradeName)
            .ThenBy(b => b.ExpiryDate)
            .ToListAsync();

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
        var items = await _unitOfWork.GetRepository<Medicine>().Query()
            .OrderBy(m => m.TradeName)
            .AsNoTracking()
            .Select(m => new InventoryReportItemViewModel
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
            })
            .ToListAsync();

        return items;
    }

    public async Task<IReadOnlyList<MedicineViewModel>> GetLowStockReportAsync()
    {
        var today = DateTime.Today;
        var items = await _unitOfWork.GetRepository<Medicine>().Query()
            .AsNoTracking()
            .Where(m => m.MedicineBatches.Sum(b => (b.ExpiryDate > today && b.CurrentQuantity > 0) ? b.CurrentQuantity : 0) < m.MinStockLevel)
            .Select(m => new MedicineViewModel
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
            })
            .ToListAsync();

        return items;
    }

    public async Task<IReadOnlyList<MedicineViewModel>> GetExpiryReportAsync()
    {
        var today = DateTime.Today;
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);
        var items = await _unitOfWork.GetRepository<Medicine>().Query()
            .AsNoTracking()
            .Where(m => m.MedicineBatches.Any(b => b.ExpiryDate <= nearExpiry && b.ExpiryDate >= today && b.CurrentQuantity > 0))
            .Select(m => new MedicineViewModel
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
            })
            .ToListAsync();

        return items;
    }

    public async Task<IReadOnlyList<ExpiredMedicineReportItemViewModel>> GetExpiredMedicinesReportAsync()
    {
        var today = DateTime.Today;
        var batches = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .AsNoTracking()
            .Where(b => b.ExpiryDate < today && b.CurrentQuantity > 0)
            .OrderBy(b => b.ExpiryDate)
            .Select(b => new ExpiredMedicineReportItemViewModel
            {
                MedicineName = b.Medicine.TradeName,
                BatchNumber = b.BatchNumber,
                ExpiryDate = b.ExpiryDate,
                Quantity = b.CurrentQuantity,
                LossValue = b.CurrentQuantity * b.PurchasePrice / Math.Max(1, b.Medicine.UnitsPerPurchaseUnit)
            })
            .ToListAsync();

        return batches;
    }

    public async Task<IReadOnlyList<SalesReportItemViewModel>> GetSalesReportAsync(
        ReportPeriod period = ReportPeriod.Monthly,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (start, end) = GetDateRange(period, from, to);
        var items = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .AsNoTracking()
            .Where(s => s.InvoiceDate >= start && s.InvoiceDate < end)
            .OrderByDescending(s => s.InvoiceDate)
            .Select(s => new SalesReportItemViewModel
            {
                InvoiceId = s.Id,
                InvoiceDate = s.InvoiceDate,
                CustomerName = s.Customer.Name,
                SaleType = s.SaleType.ToString(),
                TotalAmount = s.TotalAmount
            })
            .ToListAsync();

        return items;
    }

    public async Task<IReadOnlyList<PurchaseReportItemViewModel>> GetPurchaseReportAsync(
        ReportPeriod period = ReportPeriod.Monthly,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (start, end) = GetDateRange(period, from, to);
        var items = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .AsNoTracking()
            .Where(p => p.InvoiceDate >= start && p.InvoiceDate < end)
            .OrderByDescending(p => p.InvoiceDate)
            .Select(p => new PurchaseReportItemViewModel
            {
                InvoiceId = p.Id,
                InvoiceDate = p.InvoiceDate,
                SupplierName = p.Supplier.Name,
                TotalAmount = p.TotalAmount
            })
            .ToListAsync();

        return items;
    }

    public async Task<ProfitReportViewModel> GetProfitReportAsync(
        ReportPeriod period = ReportPeriod.Monthly,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (start, end) = GetDateRange(period, from, to);
        
        var sales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .AsNoTracking()
            .Where(s => s.InvoiceDate >= start && s.InvoiceDate < end)
            .SumAsync(s => s.TotalAmount);
            
        var purchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .AsNoTracking()
            .Where(p => p.InvoiceDate >= start && p.InvoiceDate < end)
            .SumAsync(p => p.TotalAmount);

        var profit = sales - purchases;

        return new ProfitReportViewModel
        {
            TotalSales = sales,
            TotalPurchases = purchases,
            Profit = profit,
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
        var items = await _unitOfWork.GetRepository<SalesInvoiceItem>().Query()
            .AsNoTracking()
            .Where(i => i.SalesInvoice.InvoiceDate >= start && i.SalesInvoice.InvoiceDate < end)
            .GroupBy(i => new { i.MedicineId, i.Medicine.TradeName })
            .Select(g => new TopSellingMedicineViewModel
            {
                MedicineName = g.Key.TradeName,
                QuantitySold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(20)
            .ToListAsync();

        return items;
    }

    public async Task<IReadOnlyList<SalesByCategoryViewModel>> GetSalesByCategoryReportAsync(
        ReportPeriod period = ReportPeriod.Monthly,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (start, end) = GetDateRange(period, from, to);
        var items = await _unitOfWork.GetRepository<SalesInvoiceItem>().Query()
            .AsNoTracking()
            .Where(i => i.SalesInvoice.InvoiceDate >= start && i.SalesInvoice.InvoiceDate < end)
            .GroupBy(i => i.Medicine.Category.Name)
            .Select(g => new SalesByCategoryViewModel
            {
                CategoryName = g.Key,
                TotalSales = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.TotalSales)
            .ToListAsync();

        return items;
    }

    public async Task<IReadOnlyList<FinishedMedicineReportItemViewModel>> GetFinishedMedicinesReportAsync()
    {
        var today = DateTime.Today;
        var items = await _unitOfWork.GetRepository<Medicine>().Query()
            .AsNoTracking()
            .Where(m => !m.MedicineBatches.Any(b => b.ExpiryDate > today && b.CurrentQuantity > 0))
            .OrderBy(m => m.TradeName)
            .Select(m => new FinishedMedicineReportItemViewModel
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
            .ToListAsync();

        return items;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);
        var todayUtc = DateTime.UtcNow.Date;

        var todaySales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Where(s => s.InvoiceDate >= today && s.InvoiceDate < tomorrow)
            .SumAsync(s => s.TotalAmount);

        var monthlySales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Where(s => s.InvoiceDate >= monthStart && s.InvoiceDate < tomorrow)
            .SumAsync(s => s.TotalAmount);

        var todayPurchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .Where(p => p.InvoiceDate >= today && p.InvoiceDate < tomorrow)
            .SumAsync(p => p.TotalAmount);

        var monthlyPurchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .Where(p => p.InvoiceDate >= monthStart && p.InvoiceDate < tomorrow)
            .SumAsync(p => p.TotalAmount);

        var batches = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .Include(b => b.Medicine)
            .Where(b => b.CurrentQuantity > 0)
            .ToListAsync();

        var inventoryValue = batches.Sum(b =>
            b.CurrentQuantity * b.PurchasePrice / Math.Max(1, b.Medicine.UnitsPerPurchaseUnit));

        return new DashboardViewModel
        {
            TotalMedicines = await _unitOfWork.GetRepository<Medicine>().Query().CountAsync(),
            TotalCategories = await _unitOfWork.GetRepository<Category>().Query().CountAsync(),
            TotalSuppliers = await _unitOfWork.GetRepository<Supplier>().Query().CountAsync(),
            TotalCustomers = await _unitOfWork.GetRepository<Customer>().Query().CountAsync(),
            LowStockCount = await _unitOfWork.GetRepository<Medicine>().Query()
                .CountAsync(m => m.MedicineBatches.Sum(b => (b.ExpiryDate > today && b.CurrentQuantity > 0) ? b.CurrentQuantity : 0) < m.MinStockLevel),
            CustomersToday = await _unitOfWork.GetRepository<Customer>().Query()
                .CountAsync(c => c.CreatedAt >= todayUtc),
            ActiveUsers = await _unitOfWork.Context.Set<ApplicationUser>().CountAsync(),
            UnreadNotifications = await _notificationService.GetUnreadCountAsync(),
            ExpiredCount = await _unitOfWork.GetRepository<MedicineBatch>().Query()
                .CountAsync(b => b.ExpiryDate < today && b.CurrentQuantity > 0),
            NearExpiryCount = await _unitOfWork.GetRepository<MedicineBatch>().Query()
                .CountAsync(b => b.ExpiryDate >= today && b.ExpiryDate <= nearExpiry && b.CurrentQuantity > 0),
            TodaySales = todaySales,
            MonthlySales = monthlySales,
            TodayPurchases = todayPurchases,
            MonthlyPurchases = monthlyPurchases,
            TodayProfit = todaySales - todayPurchases,
            MonthlyProfit = monthlySales - monthlyPurchases,
            SalesReturnCount = await _salesReturnService.GetReturnCountAsync(),
            InventoryTotalValue = inventoryValue
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
