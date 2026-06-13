using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.DAL.Data.Entities;

public class SalesInvoice : BaseEntity
{
    public DateTime InvoiceDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string? DoctorName { get; set; }
    public string? CreatedByUserId { get; set; }
    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    public ICollection<SalesReturn> SalesReturns { get; set; } = new List<SalesReturn>();
    
    public SaleType SaleType { get; set; } = SaleType.Cash;
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
    
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
