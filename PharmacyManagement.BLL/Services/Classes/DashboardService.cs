using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.DashboardViewModels;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonthStart = thisMonthStart.AddMonths(1);

        var inThreeMonths = today.AddMonths(3);
        var data = await _dashboardRepository.GetDashboardDataAsync(today, thisMonthStart, nextMonthStart, inThreeMonths);

        var vm = new DashboardViewModel
        {
            TodaySales = data.TodaySales,
            TodayPurchases = data.TodayPurchases,
            TodayProfit = data.TodaySales - data.TodayPurchases,
            MonthlySales = data.MonthlySales,
            MonthlyPurchases = data.MonthlyPurchases,
            MonthlyProfit = data.MonthlySales - data.MonthlyPurchases,
            TotalCustomers = data.TotalCustomers,
            OutstandingDebt = data.OutstandingDebt,
            TotalProducts = data.TotalProducts,
            LowStockItems = data.LowStockItems,
            NearLowStockItems = data.NearLowStockItems,
            ExpiringMedicines = data.ExpiringMedicines,
            NearExpiringMedicines = data.NearExpiringMedicines
        };

        var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-i)).Reverse().ToList();
        foreach (var date in last7Days)
        {
            var daySales = data.RecentSalesByDate.GetValueOrDefault(date);
            vm.SalesTrend.Add(new SalesTrendPoint { Date = date.ToString("MMM dd"), Amount = daySales });
        }

        vm.RecentActivities = data.RecentActivities.Select(a => new RecentActivityDto
        {
            ActivityType = a.ActivityType,
            Description = a.Description,
            Time = a.Time,
            Amount = a.Amount,
            Icon = a.Icon,
            Color = a.Color
        }).ToList();

        return vm;
    }
}
