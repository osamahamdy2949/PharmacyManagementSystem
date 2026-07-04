using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.DashboardViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var vm = new DashboardViewModel();

        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonthStart = thisMonthStart.AddMonths(1);

        vm.TodaySales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .AsNoTracking()
            .Where(s => s.InvoiceDate >= today && s.InvoiceDate < tomorrow)
            .SumAsync(s => s.TotalAmount);

        vm.TodayPurchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .AsNoTracking()
            .Where(p => p.InvoiceDate >= today && p.InvoiceDate < tomorrow)
            .SumAsync(p => p.TotalAmount);

        vm.TodayProfit = vm.TodaySales - vm.TodayPurchases;

        vm.MonthlySales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .AsNoTracking()
            .Where(s => s.InvoiceDate >= thisMonthStart && s.InvoiceDate < nextMonthStart)
            .SumAsync(s => s.TotalAmount);

        vm.MonthlyPurchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .AsNoTracking()
            .Where(p => p.InvoiceDate >= thisMonthStart && p.InvoiceDate < nextMonthStart)
            .SumAsync(p => p.TotalAmount);

        vm.MonthlyProfit = vm.MonthlySales - vm.MonthlyPurchases;

        vm.TotalCustomers = await _unitOfWork.GetRepository<Customer>().Query().AsNoTracking().CountAsync();
        vm.OutstandingDebt = await _unitOfWork.GetRepository<Customer>().Query().AsNoTracking().SumAsync(c => c.RemainingBalance);
        vm.TotalProducts = await _unitOfWork.GetRepository<Medicine>().Query().AsNoTracking().CountAsync();

        var inThreeMonths = today.AddMonths(3);

        vm.LowStockItems = await _unitOfWork.GetRepository<Medicine>().Query()
            .AsNoTracking()
            .CountAsync(m => !m.MedicineBatches.Any(b => b.ExpiryDate > today && b.CurrentQuantity > 0));

        vm.NearLowStockItems = await _unitOfWork.GetRepository<Medicine>().Query()
            .AsNoTracking()
            .CountAsync(m =>
                m.MedicineBatches.Where(b => b.ExpiryDate > today).Sum(b => b.CurrentQuantity) > 0 &&
                m.MedicineBatches.Where(b => b.ExpiryDate > today).Sum(b => b.CurrentQuantity) <= m.MinStockLevel);

        vm.ExpiringMedicines = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .AsNoTracking()
            .CountAsync(b => b.ExpiryDate < today && b.CurrentQuantity > 0);

        vm.NearExpiringMedicines = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .AsNoTracking()
            .CountAsync(b => b.ExpiryDate >= today && b.ExpiryDate <= inThreeMonths && b.CurrentQuantity > 0);

        var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-i)).Reverse().ToList();
        var recentSales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .AsNoTracking()
            .Where(s => s.InvoiceDate >= today.AddDays(-6) && s.InvoiceDate < tomorrow)
            .GroupBy(s => s.InvoiceDate.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(s => s.TotalAmount) })
            .ToDictionaryAsync(g => g.Date, g => g.Amount);

        foreach (var date in last7Days)
        {
            var daySales = recentSales.GetValueOrDefault(date);
            vm.SalesTrend.Add(new SalesTrendPoint { Date = date.ToString("MMM dd"), Amount = daySales });
        }

        var activities = new List<RecentActivityDto>();

        var latestSales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt).Take(3)
            .Select(s => new RecentActivityDto { ActivityType = "Sale", Description = $"Sale Invoice #{s.Id}", Time = s.CreatedAt, Amount = s.TotalAmount, Icon = "bi-cart-check", Color = "text-success bg-success-subtle" })
            .ToListAsync();

        var latestPurchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt).Take(2)
            .Select(p => new RecentActivityDto { ActivityType = "Purchase", Description = $"Purchase Invoice #{p.Id}", Time = p.CreatedAt, Amount = p.TotalAmount, Icon = "bi-bag-plus", Color = "text-danger bg-danger-subtle" })
            .ToListAsync();

        var latestPayments = await _unitOfWork.GetRepository<Payment>().Query()
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt).Take(2)
            .Select(p => new RecentActivityDto { ActivityType = "Payment", Description = $"Payment from {p.Customer.Name}", Time = p.CreatedAt, Amount = p.AmountPaid, Icon = "bi-cash-coin", Color = "text-info bg-info-subtle" })
            .ToListAsync();

        activities.AddRange(latestSales);
        activities.AddRange(latestPurchases);
        activities.AddRange(latestPayments);

        vm.RecentActivities = activities.OrderByDescending(a => a.Time).Take(6).ToList();

        return vm;
    }
}
