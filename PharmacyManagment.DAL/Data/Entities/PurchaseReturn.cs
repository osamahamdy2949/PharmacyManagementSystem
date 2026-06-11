using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.DAL.Data.Entities;

public class PurchaseReturn : BaseEntity
{
    public int PurchaseInvoiceId { get; set; }
    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public DateTime ReturnDate { get; set; }
    public PurchaseReturnReason Reason { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CreatedByUserId { get; set; }
    public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
}
