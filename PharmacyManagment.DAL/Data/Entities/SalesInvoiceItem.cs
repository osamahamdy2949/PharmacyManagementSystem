namespace PharmacyManagement.DAL.Data.Entities;

public class SalesInvoiceItem : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public int MedicineBatchId { get; set; }
    public MedicineBatch MedicineBatch { get; set; } = null!;
    public int SalesInvoiceId { get; set; }
    public SalesInvoice SalesInvoice { get; set; } = null!;
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
}
