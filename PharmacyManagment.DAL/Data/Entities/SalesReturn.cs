namespace PharmacyManagement.DAL.Data.Entities;

public class SalesReturn : BaseEntity
{
    public int SalesInvoiceId { get; set; }
    public SalesInvoice SalesInvoice { get; set; } = null!;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime ReturnDate { get; set; }
    public string? Reason { get; set; }
    public decimal RefundAmount { get; set; }
    public string? CreatedByUserId { get; set; }
    public ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
}
