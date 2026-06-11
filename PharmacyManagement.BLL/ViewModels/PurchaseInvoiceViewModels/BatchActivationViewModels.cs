using System.ComponentModel.DataAnnotations;
using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.BLL.ViewModels.PurchaseInvoiceViewModels;

public class PendingBatchViewModel
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

public class ActivateBatchViewModel
{
    public int BatchId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public int PendingQuantity { get; set; }

    [Required(ErrorMessage = "Barcodes are required.")]
    [Display(Name = "Barcodes / Serials")]
    public List<string> Barcodes { get; set; } = new();

    [Required]
    [DataType(DataType.Date)]
    [FutureDate(ErrorMessage = "Expiry Date must be greater than today.")]
    [Display(Name = "Expiry Date")]
    public DateTime? ExpiryDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [PastOrTodayDate(ErrorMessage = "Manufacture Date must be today or in the past.")]
    [Display(Name = "Manufacture Date")]
    public DateTime? ManufactureDate { get; set; }

    [Required(ErrorMessage = "Purchase Unit is required.")]
    [Display(Name = "Purchase Unit")]
    public PurchaseUnit? PurchaseUnit { get; set; }

    [Required(ErrorMessage = "Sale Unit is required.")]
    [Display(Name = "Sale Unit")]
    public SaleUnit? SaleUnit { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Sale units per purchase unit must be greater than 0.")]
    [Display(Name = "Sale units per purchase unit")]
    public int UnitsPerPurchaseUnit { get; set; } = 0;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Purchase Price must be greater than 0.")]
    [Display(Name = "Purchase Price (per purchase unit)")]
    public decimal PurchasePrice { get; set; } = 0;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Selling Price must be greater than 0.")]
    [Display(Name = "Selling Price (per sale unit)")]
    public decimal SellingPrice { get; set; } = 0;

    [Required, Range(1, int.MaxValue)]
    [Display(Name = "Low Stock Alarm (sale units)")]
    public int MinStockLevel { get; set; } = 10;
}

public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is DateTime dt)
            return dt.Date > DateTime.Today;
        return false;
    }
}

public class PastOrTodayDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is DateTime dt)
            return dt.Date <= DateTime.Today;
        return false;
    }
}

