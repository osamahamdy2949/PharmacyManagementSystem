namespace PharmacyManagement.DAL.Data.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public decimal TotalSpent { get; set; }
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
}
