namespace PharmacyManagement.DAL.Data.Entities;

public class PurchaseInvoice : BaseEntity
{
    public DateTime InvoiceDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public string? CreatedByUserId { get; set; }
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
    public ICollection<PurchaseReturn> PurchaseReturns { get; set; } = new List<PurchaseReturn>();
}
