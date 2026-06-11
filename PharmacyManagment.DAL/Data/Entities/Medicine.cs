using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.DAL.Data.Entities;

public class Medicine : BaseEntity
{
    public string SerialNumber { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MedicineForm MedicineForm { get; set; }
    public PurchaseUnit PurchaseUnit { get; set; }
    public SaleUnit SaleUnit { get; set; }
    public int UnitsPerPurchaseUnit { get; set; } = 1;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal StrengthValue { get; set; }
    public string StrengthUnit { get; set; } = string.Empty;
    public int MinStockLevel { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public ICollection<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; } = new List<PurchaseInvoiceItem>();
    public ICollection<SalesInvoiceItem> SalesInvoiceItems { get; set; } = new List<SalesInvoiceItem>();
    public ICollection<MedicineBatch> MedicineBatches { get; set; } = new List<MedicineBatch>();
}
