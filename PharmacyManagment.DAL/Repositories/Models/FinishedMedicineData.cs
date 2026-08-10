using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.DAL.Repositories.Models;

public class FinishedMedicineData
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string MedicineForm { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
    public string SaleUnit { get; set; } = string.Empty;
    public int UnitsPerPurchaseUnit { get; set; }
    public decimal PurchasePricePerPurchaseUnit { get; set; }
}
