using System.ComponentModel.DataAnnotations;
using PharmacyManagement.BLL.ViewModels.InvoiceItemViewModels;
using PharmacyManagement.DAL.Data.Entities.Enums;
using Enums = PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.BLL.ViewModels.PurchaseInvoiceViewModels;

public class PurchaseInvoiceViewModel
{
    public int Id { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Invoice Date")]
    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    [Display(Name = "Total Amount")]
    public decimal TotalAmount { get; set; }

    [Required]
    [Display(Name = "Supplier")]
    public int SupplierId { get; set; }

    public string? SupplierName { get; set; }
    public List<InvoiceItemViewModel> Items { get; set; } = new();
}

public class CreatePurchaseInvoiceViewModel
{
    [Required]
    public int SupplierId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    public List<PurchaseLineItemViewModel> Items { get; set; } = new() { new() };
}

public class PurchaseLineItemViewModel
{
    public int? MedicineId { get; set; }

    public int Quantity { get; set; } = 1;

    [Display(Name = "Dose")]
    public decimal? DoseValue { get; set; }

    public DoseUnit? DoseUnit { get; set; }

    public string Dose { get; set; } = string.Empty;

    public string BatchNumber { get; set; } = string.Empty;

    public void BuildDose()
    {
        if (!DoseValue.HasValue || DoseValue <= 0 || !DoseUnit.HasValue)
            return;

        var unitLabel = DoseUnit.Value == Enums.DoseUnit.Percent ? "%" : DoseUnit.Value.ToString();
        Dose = $"{DoseValue.Value:g} {unitLabel}";
    }
}
