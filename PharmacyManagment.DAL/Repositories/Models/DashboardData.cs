namespace PharmacyManagement.DAL.Repositories.Models;

public class DashboardData
{
    public decimal TodaySales { get; set; }
    public decimal TodayPurchases { get; set; }
    public decimal MonthlySales { get; set; }
    public decimal MonthlyPurchases { get; set; }
    public int TotalCustomers { get; set; }
    public decimal OutstandingDebt { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockItems { get; set; }
    public int NearLowStockItems { get; set; }
    public int ExpiringMedicines { get; set; }
    public int NearExpiringMedicines { get; set; }
    public IReadOnlyDictionary<DateTime, decimal> RecentSalesByDate { get; set; } = new Dictionary<DateTime, decimal>();
    public IReadOnlyList<RecentActivityData> RecentActivities { get; set; } = Array.Empty<RecentActivityData>();
}
