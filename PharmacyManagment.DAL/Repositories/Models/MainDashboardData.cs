namespace PharmacyManagement.DAL.Repositories.Models;

public class MainDashboardData
{
    public int TotalMedicines { get; set; }
    public int NearExpiryCount { get; set; }
    public int ExpiredCount { get; set; }
    public int LowStockCount { get; set; }
    public int TotalCategories { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalCustomers { get; set; }
    public int CustomersToday { get; set; }
    public int ActiveUsers { get; set; }
    public decimal TodaySales { get; set; }
    public decimal MonthlySales { get; set; }
    public decimal TodayPurchases { get; set; }
    public decimal MonthlyPurchases { get; set; }
    public decimal InventoryTotalValue { get; set; }
}
