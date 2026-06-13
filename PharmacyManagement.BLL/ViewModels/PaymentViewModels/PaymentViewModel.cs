using System.ComponentModel.DataAnnotations;
using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.BLL.ViewModels.PaymentViewModels;

public class PaymentViewModel
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? SalesInvoiceId { get; set; }
    
    [DisplayFormat(DataFormatString = "{0:N2}")]
    public decimal AmountPaid { get; set; }
    
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? RecordedByUserName { get; set; }
}
