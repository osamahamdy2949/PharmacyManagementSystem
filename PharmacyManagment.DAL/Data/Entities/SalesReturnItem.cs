namespace PharmacyManagement.DAL.Data.Entities;

public class SalesReturnItem : BaseEntity
{
    public int SalesReturnId { get; set; }
    public SalesReturn SalesReturn { get; set; } = null!;
    public int MedicineBatchId { get; set; }
    public MedicineBatch MedicineBatch { get; set; } = null!;
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Reason { get; set; }
}
