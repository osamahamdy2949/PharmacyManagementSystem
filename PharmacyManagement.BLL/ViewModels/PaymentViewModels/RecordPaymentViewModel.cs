using System.ComponentModel.DataAnnotations;
using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.BLL.ViewModels.PaymentViewModels;

public class RecordPaymentViewModel
{
    [Required]
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal CustomerTotalDebt { get; set; }

    public int? SalesInvoiceId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal AmountPaid { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [Required]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [StringLength(500)]
    public string? Notes { get; set; }
}
