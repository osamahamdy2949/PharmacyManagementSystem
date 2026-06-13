using AutoMapper;
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
    private readonly IMapper _mapper;
    private readonly IStockService _stockService;
    private readonly ISalesReturnService _salesReturnService;
    private readonly INotificationService _notificationService;

    public ReportService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IStockService stockService,
        ISalesReturnService salesReturnService,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _stockService = stockService;
        _salesReturnService = salesReturnService;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<BatchInventoryItemViewModel>> GetBatchInventoryAsync(InventoryFilter filter = InventoryFilter.All)
    {
        var today = DateTime.Today;
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);
        var batches = await _unitOfWork.GetRepository<MedicineBatch>().Query()
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
        var items = await _unitOfWork.GetRepository<Medicine>().Query()
            .Include(m => m.Category)
            .OrderBy(m => m.TradeName)
            .ToListAsync();
        var vms = _mapper.Map<IReadOnlyList<InventoryReportItemViewModel>>(items);
        foreach (var vm in vms)
            vm.QuantityInStock = await _stockService.GetAvailableStockAsync(vm.Id);
        return vms;
    }

    public async Task<IReadOnlyList<MedicineViewModel>> GetLowStockReportAsync()
    {
        var today = DateTime.Today;
        var items = await _unitOfWork.GetRepository<Medicine>().Query()
            .Include(m => m.Category)
            .Where(m => m.MedicineBatches.Sum(b => (b.ExpiryDate > today && b.CurrentQuantity > 0) ? b.CurrentQuantity : 0) < m.MinStockLevel)
            .ToListAsync();
        var vms = _mapper.Map<IReadOnlyList<MedicineViewModel>>(items);
        foreach (var vm in vms)
            vm.QuantityInStock = await _stockService.GetAvailableStockAsync(vm.Id);
        return vms;
    }

    public async Task<IReadOnlyList<MedicineViewModel>> GetExpiryReportAsync()
    {
        var today = DateTime.Today;
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);
        var items = await _unitOfWork.GetRepository<Medicine>().Query()
            .Include(m => m.Category)
            .Where(m => m.MedicineBatches.Any(b => b.ExpiryDate <= nearExpiry && b.ExpiryDate >= today && b.CurrentQuantity > 0))
            .ToListAsync();
        var vms = _mapper.Map<IReadOnlyList<MedicineViewModel>>(items);
        foreach (var vm in vms)
        {
            vm.QuantityInStock = await _stockService.GetAvailableStockAsync(vm.Id);
            vm.IsNearExpiry = true;
        }
        return vms;
    }

    public async Task<IReadOnlyList<ExpiredMedicineReportItemViewModel>> GetExpiredMedicinesReportAsync()
    {
        var today = DateTime.Today;
        var batches = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .Include(b => b.Medicine)
            .Where(b => b.ExpiryDate < today && b.CurrentQuantity > 0)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

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
        var items = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Include(s => s.Customer)
            .Where(s => s.InvoiceDate >= start && s.InvoiceDate < end)
            .OrderByDescending(s => s.InvoiceDate)
            .ToListAsync();
        return _mapper.Map<IReadOnlyList<SalesReportItemViewModel>>(items);
    }

    public async Task<IReadOnlyList<PurchaseReportItemViewModel>> GetPurchaseReportAsync(
        ReportPeriod period = ReportPeriod.Monthly,
        DateTime? from = null,
        DateTime? to = null)
    {
        var (start, end) = GetDateRange(period, from, to);
        var items = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .Include(p => p.Supplier)
            .Where(p => p.InvoiceDate >= start && p.InvoiceDate < end)
            .OrderByDescending(p => p.InvoiceDate)
            .ToListAsync();

        return items.Select(p => new PurchaseReportItemViewModel
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
        
        var sales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Where(s => s.InvoiceDate >= start && s.InvoiceDate < end)
            .SumAsync(s => s.TotalAmount);
            
        var purchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .Where(p => p.InvoiceDate >= start && p.InvoiceDate < end)
            .SumAsync(p => p.TotalAmount);

        // Calculate gross profit based on sold items (Selling Price - Purchase Price) * Quantity
        var soldItems = await _unitOfWork.Context.Set<SalesInvoiceItem>()
            .Include(i => i.Medicine)
            .Where(i => i.SalesInvoice.InvoiceDate >= start && i.SalesInvoice.InvoiceDate < end)
            .ToListAsync();
            
        var grossProfit = soldItems.Sum(i => (i.UnitPrice - i.Medicine.PurchasePrice) * i.Quantity);

        return new ProfitReportViewModel
        {
            TotalSales = sales,
            TotalPurchases = purchases,
            Profit = grossProfit, // Assuming we add a setter to Profit in ViewModel
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
            .Include(i => i.Medicine)
            .Include(i => i.SalesInvoice)
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
            .Include(i => i.Medicine).ThenInclude(m => m.Category)
            .Include(i => i.SalesInvoice)
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
            .Include(m => m.Category)
            .Where(m => !m.MedicineBatches.Any(b => b.ExpiryDate > today && b.CurrentQuantity > 0))
            .OrderBy(m => m.TradeName)
            .ToListAsync();
        return _mapper.Map<IReadOnlyList<FinishedMedicineReportItemViewModel>>(items);
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);
        var todayUtc = DateTime.UtcNow.Date;

        var todaySales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Where(s => s.InvoiceDate == today).SumAsync(s => s.TotalAmount);
        var monthlySales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Where(s => s.InvoiceDate >= monthStart && s.InvoiceDate <= today).SumAsync(s => s.TotalAmount);
        var todayPurchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .Where(p => p.InvoiceDate == today).SumAsync(p => p.TotalAmount);
        var monthlyPurchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .Where(p => p.InvoiceDate >= monthStart && p.InvoiceDate <= today).SumAsync(p => p.TotalAmount);

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
