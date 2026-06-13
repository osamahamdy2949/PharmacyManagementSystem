namespace PharmacyManagement.BLL.ViewModels.CustomerViewModels;

public class CustomerDebtHistoryViewModel
{
    public int InvoiceId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}
