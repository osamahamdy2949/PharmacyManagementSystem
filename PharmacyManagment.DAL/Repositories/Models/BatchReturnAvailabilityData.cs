namespace PharmacyManagement.DAL.Repositories.Models;

public class BatchReturnAvailabilityData
{
    public int BatchId { get; set; }
    public int MedicineId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
}
