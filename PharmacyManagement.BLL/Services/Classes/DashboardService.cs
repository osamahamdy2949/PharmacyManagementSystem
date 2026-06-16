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

        var todaySales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Where(s => s.CreatedAt >= today && s.CreatedAt < tomorrow)
            .ToListAsync();
        vm.TodaySales = todaySales.Sum(s => s.TotalAmount);

        var todayPurchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .Where(p => p.CreatedAt >= today && p.CreatedAt < tomorrow)
            .ToListAsync();
        vm.TodayPurchases = todayPurchases.Sum(p => p.TotalAmount);

        var todayItems = await _unitOfWork.Context.Set<SalesInvoiceItem>()
            .Include(i => i.Medicine)
            .Where(i => i.SalesInvoice.CreatedAt >= today && i.SalesInvoice.CreatedAt < tomorrow)
            .ToListAsync();
        vm.TodayProfit = todayItems.Sum(i => (i.UnitPrice - i.Medicine.PurchasePrice) * i.Quantity);

        var monthSales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Where(s => s.CreatedAt >= thisMonthStart && s.CreatedAt < nextMonthStart)
            .ToListAsync();
        vm.MonthlySales = monthSales.Sum(s => s.TotalAmount);

        var monthPurchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .Where(p => p.CreatedAt >= thisMonthStart && p.CreatedAt < nextMonthStart)
            .ToListAsync();
        vm.MonthlyPurchases = monthPurchases.Sum(p => p.TotalAmount);

        var monthItems = await _unitOfWork.Context.Set<SalesInvoiceItem>()
            .Include(i => i.Medicine)
            .Where(i => i.SalesInvoice.CreatedAt >= thisMonthStart && i.SalesInvoice.CreatedAt < nextMonthStart)
            .ToListAsync();
        vm.MonthlyProfit = monthItems.Sum(i => (i.UnitPrice - i.Medicine.PurchasePrice) * i.Quantity);

        vm.TotalCustomers = await _unitOfWork.GetRepository<Customer>().Query().CountAsync();
        vm.OutstandingDebt = await _unitOfWork.GetRepository<Customer>().Query().SumAsync(c => c.RemainingBalance);
        vm.TotalProducts = await _unitOfWork.GetRepository<Medicine>().Query().CountAsync();

        var allMedicines = await _unitOfWork.Context.Set<Medicine>().Include(m => m.MedicineBatches).ToListAsync();
        vm.LowStockItems = allMedicines.Count(m => m.MedicineBatches.Sum(b => b.CurrentQuantity) == 0);
        vm.NearLowStockItems = allMedicines.Count(m => m.MedicineBatches.Sum(b => b.CurrentQuantity) > 0 && m.MedicineBatches.Sum(b => b.CurrentQuantity) <= m.MinStockLevel);

        var allBatches = await _unitOfWork.Context.Set<MedicineBatch>().Where(b => b.CurrentQuantity > 0).ToListAsync();
        var inThreeMonths = today.AddMonths(3);

        vm.ExpiringMedicines = allBatches.Count(b => b.ExpiryDate <= today);
        vm.NearExpiringMedicines = allBatches.Count(b => b.ExpiryDate > today && b.ExpiryDate <= inThreeMonths);

        var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-i)).Reverse().ToList();
        var recentSales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Where(s => s.CreatedAt >= today.AddDays(-6) && s.CreatedAt < tomorrow)
            .ToListAsync();

        foreach (var date in last7Days)
        {
            var daySales = recentSales.Where(s => s.CreatedAt.Date == date).Sum(s => s.TotalAmount);
            vm.SalesTrend.Add(new SalesTrendPoint { Date = date.ToString("MMM dd"), Amount = daySales });
        }

        var activities = new List<RecentActivityDto>();

        var latestSales = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .OrderByDescending(s => s.CreatedAt).Take(3)
            .Select(s => new RecentActivityDto { ActivityType = "Sale", Description = $"Sale Invoice #{s.Id}", Time = s.CreatedAt, Amount = s.TotalAmount, Icon = "bi-cart-check", Color = "text-success bg-success-subtle" })
            .ToListAsync();

        var latestPurchases = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .OrderByDescending(p => p.CreatedAt).Take(2)
            .Select(p => new RecentActivityDto { ActivityType = "Purchase", Description = $"Purchase Invoice #{p.Id}", Time = p.CreatedAt, Amount = p.TotalAmount, Icon = "bi-bag-plus", Color = "text-danger bg-danger-subtle" })
            .ToListAsync();

        var latestPayments = await _unitOfWork.GetRepository<Payment>().Query()
            .Include(p => p.Customer)
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
