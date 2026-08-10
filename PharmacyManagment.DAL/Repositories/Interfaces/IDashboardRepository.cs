using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardData> GetDashboardDataAsync(DateTime today, DateTime thisMonthStart, DateTime nextMonthStart, DateTime inThreeMonths, CancellationToken ct = default);
    Task<MainDashboardData> GetMainDashboardDataAsync(DateTime today, DateTime tomorrow, DateTime monthStart, DateTime nearExpiry, DateTime todayUtc, CancellationToken ct = default);
}
