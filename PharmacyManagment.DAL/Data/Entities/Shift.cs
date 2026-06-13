namespace PharmacyManagement.DAL.Data.Entities;

public class Shift : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public decimal? ClosingCash { get; set; }
    public decimal? CashDifference { get; set; }
    
    public decimal TotalSales { get; set; }
    public decimal CashSales { get; set; }
    public decimal CreditSales { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalPaymentsReceived { get; set; }
    
    public decimal NetCash { get; set; }
    
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
