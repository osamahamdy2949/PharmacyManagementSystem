using System.ComponentModel.DataAnnotations;
using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.BLL.ViewModels.MedicineViewModels;

public class MedicineViewModel
{
    public int Id { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Serial Number")]
    public string SerialNumber { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Trade Name")]
    public string TradeName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Scientific Name")]
    public string ScientificName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Form")]
    public MedicineForm MedicineForm { get; set; } = MedicineForm.Tablets;

    [Required]
    [Display(Name = "Purchase Unit (e.g. Box)")]
    public PurchaseUnit PurchaseUnit { get; set; } = PurchaseUnit.Box;

    [Required]
    [Display(Name = "Sale Unit (e.g. Strip)")]
    public SaleUnit SaleUnit { get; set; } = SaleUnit.Strip;

    [Required, Range(1, int.MaxValue)]
    [Display(Name = "Sale units per purchase unit")]
    public int UnitsPerPurchaseUnit { get; set; } = 1;

    [Required, Range(0.01, double.MaxValue)]
    [Display(Name = "Purchase Price (per purchase unit)")]
    public decimal PurchasePrice { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    [Display(Name = "Selling Price (per sale unit)")]
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// Computed from MedicineBatch sum — not stored on entity.
    /// </summary>
    [Display(Name = "Quantity In Stock (sale units)")]
    public int QuantityInStock { get; set; }

    public string UnitInfo => $"1 {PurchaseUnit} = {UnitsPerPurchaseUnit} {SaleUnit}(s)";

    [Required, StringLength(50)]
    public string Manufacturer { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Barcode { get; set; }

    [Display(Name = "Strength")]
    public decimal StrengthValue { get; set; }

    [StringLength(20)]
    [Display(Name = "Strength Unit")]
    public string StrengthUnit { get; set; } = string.Empty;

    [Display(Name = "Minimum Stock Level")]
    [Range(1, int.MaxValue)]
    public int MinStockLevel { get; set; } = Common.ValidationConstants.LowStockThreshold;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public bool IsLowStock => QuantityInStock < MinStockLevel;

    /// <summary>
    /// Set by the service based on batch data.
    /// </summary>
    public bool IsNearExpiry { get; set; }

    [Display(Name = "Next Expiry Date")]
    [DataType(DataType.Date)]
    public DateTime ExpiryDate { get; set; }
}
