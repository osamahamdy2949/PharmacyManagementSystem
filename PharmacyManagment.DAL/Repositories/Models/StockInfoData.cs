namespace PharmacyManagement.DAL.Repositories.Models;

public class StockInfoData
{
    public int MedicineId { get; set; }
    public int QuantityInStock { get; set; }
    public bool IsNearExpiry { get; set; }
}
