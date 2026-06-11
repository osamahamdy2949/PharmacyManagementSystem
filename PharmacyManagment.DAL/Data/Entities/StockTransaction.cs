using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.DAL.Data.Entities;

public class StockTransaction : BaseEntity
{
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int? MedicineBatchId { get; set; }
    public MedicineBatch? MedicineBatch { get; set; }
    public StockTransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public int? ReferenceId { get; set; }
    public string? UserId { get; set; }
    public string? Notes { get; set; }
}
