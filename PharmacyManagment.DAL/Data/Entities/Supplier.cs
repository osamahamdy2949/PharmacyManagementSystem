namespace PharmacyManagement.DAL.Data.Entities;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? TaxRegNumber { get; set; }
    public ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
    public ICollection<PurchaseReturn> PurchaseReturns { get; set; } = new List<PurchaseReturn>();
}
