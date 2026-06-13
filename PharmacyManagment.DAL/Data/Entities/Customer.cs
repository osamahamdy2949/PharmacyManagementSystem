namespace PharmacyManagement.DAL.Data.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public decimal TotalSpent { get; set; }
    
    public decimal TotalDebt { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
