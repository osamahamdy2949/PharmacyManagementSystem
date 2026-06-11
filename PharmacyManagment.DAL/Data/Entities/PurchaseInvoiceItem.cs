namespace PharmacyManagement.DAL.Data.Entities;

public class PurchaseInvoiceItem : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public decimal SellingPrice { get; set; }
    public int? MedicineBatchId { get; set; }
    public MedicineBatch? MedicineBatch { get; set; }
    public int PurchaseInvoiceId { get; set; }
    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
}
