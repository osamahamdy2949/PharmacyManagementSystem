using PharmacyManagement.BLL.ViewModels.CustomerViewModels;

namespace PharmacyManagement.BLL.ViewModels.CustomerViewModels;

public class CustomerProfileViewModel : CustomerViewModel
{
    public decimal TotalSpent { get; set; }
    public decimal TotalDebt { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    
    public int TotalInvoices { get; set; }
}
