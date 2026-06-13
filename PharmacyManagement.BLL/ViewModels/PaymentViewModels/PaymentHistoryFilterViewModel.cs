namespace PharmacyManagement.BLL.ViewModels.PaymentViewModels;

public class PaymentHistoryFilterViewModel
{
    public int? CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
