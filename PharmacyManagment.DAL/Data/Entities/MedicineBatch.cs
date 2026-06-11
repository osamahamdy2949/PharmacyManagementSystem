using System.ComponentModel.DataAnnotations;
using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.DAL.Data.Entities;

public class MedicineBatch : BaseEntity
{
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public string Sku { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public PurchaseUnit PurchaseUnit { get; set; }
    public SaleUnit SaleUnit { get; set; }
    public int UnitsPerPurchaseUnit { get; set; } = 1;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int MinStockLevel { get; set; } = 10;
    /// <summary>When false, stock is pending activation (purchase units). Not counted in inventory or POS.</summary>
    public bool IsActive { get; set; }
    public int CurrentQuantity { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
