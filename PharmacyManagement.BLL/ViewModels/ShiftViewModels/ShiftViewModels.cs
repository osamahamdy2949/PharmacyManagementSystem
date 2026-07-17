using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.BLL.ViewModels.ShiftViewModels;

public class StartShiftViewModel
{
    public string? Notes { get; set; }
}

public class EndShiftViewModel
{
    public int ShiftId { get; set; }

    [Required]
    [Display(Name = "Counted Cash in Drawer")]
    [Range(-1000000, 1000000, ErrorMessage = "Please enter a valid cash amount")]
    public decimal ClosingCash { get; set; }
    
    public string? Notes { get; set; }
}

public class ShiftViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
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
    public bool IsActive { get; set; }
    
    public decimal SystemExpectedCash => CashSales + TotalPaymentsReceived - TotalPurchases;
}

public class ShiftSummaryViewModel
{
    public int ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string UserName { get; set; } = string.Empty;
    
    public decimal CashSales { get; set; }
    public decimal TotalPaymentsReceived { get; set; }
    public decimal TotalPurchases { get; set; }
    
    public decimal ExpectedCash => CashSales + TotalPaymentsReceived - TotalPurchases;
    public decimal? ActualCash { get; set; }
    public decimal? CashDifference { get; set; }
}
