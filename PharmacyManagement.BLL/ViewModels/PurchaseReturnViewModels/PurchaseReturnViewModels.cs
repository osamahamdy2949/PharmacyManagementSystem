using System.ComponentModel.DataAnnotations;
using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.BLL.ViewModels.PurchaseReturnViewModels;

public class PurchaseReturnListItemViewModel
{
    public int Id { get; set; }
    public int PurchaseInvoiceId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime ReturnDate { get; set; }
    public PurchaseReturnReason Reason { get; set; }
    public decimal TotalAmount { get; set; }
}

public class PurchaseReturnViewModel : PurchaseReturnListItemViewModel
{
    public string? Notes { get; set; }
    public List<PurchaseReturnItemViewModel> Items { get; set; } = new();
}

public class PurchaseReturnItemViewModel
{
    public int MedicineBatchId { get; set; }
    public int MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public string? BatchNumber { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

public class CreatePurchaseReturnViewModel
{
    [Required]
    public int PurchaseInvoiceId { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; } = DateTime.Today;

    [Required]
    public PurchaseReturnReason Reason { get; set; }

    public string? Notes { get; set; }
    public List<PurchaseReturnItemViewModel> Items { get; set; } = new();
}
