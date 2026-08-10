namespace PharmacyManagement.DAL.Repositories.Models;

public class PendingBatchData
{
    public int BatchId { get; set; }
    public int PurchaseInvoiceId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public int PendingQuantity { get; set; }
    public string PurchaseUnit { get; set; } = string.Empty;
}
