namespace PharmacyManagement.BLL.ViewModels.DashboardViewModels;

public class DashboardViewModel
{
    // KPI Cards - Today
    public decimal TodaySales { get; set; }
    public decimal TodayPurchases { get; set; }
    public decimal TodayProfit { get; set; }
    
    // KPI Cards - Monthly
    public decimal MonthlySales { get; set; }
    public decimal MonthlyPurchases { get; set; }
    public decimal MonthlyProfit { get; set; }
    
    // Entities KPI
    public decimal OutstandingDebt { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }

    // Alerts
    public int NearLowStockItems { get; set; }
    public int LowStockItems { get; set; }
    public int NearExpiringMedicines { get; set; }
    public int ExpiringMedicines { get; set; }
    
    // Analytics
    public List<SalesTrendPoint> SalesTrend { get; set; } = new();
    
    // Activity
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class SalesTrendPoint
{
    public string Date { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class TopProductDto
{
    public string Name { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentActivityDto
{
    public string ActivityType { get; set; } = string.Empty; // "Sale", "Purchase", "Payment", "Shift"
    public string Description { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public decimal? Amount { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
