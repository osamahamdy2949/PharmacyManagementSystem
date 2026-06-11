using PharmacyManagement.BLL.ViewModels.ReportViewModels;
using PharmacyManagement.BLL.ViewModels.SupplierViewModels;
using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.BLL.ViewModels.RestockViewModels;

public class RestockFinishedMedicineRequest
{
    public int MedicineId { get; set; }

    [Required]
    public int SupplierId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Purchase quantity must be greater than zero.")]
    public int PurchaseQuantity { get; set; }

    [Required]
    public string Dose { get; set; } = string.Empty;

    public string? BatchNumber { get; set; }
}

public class FinishedMedicinesPageViewModel
{
    public IReadOnlyList<FinishedMedicineReportItemViewModel> Medicines { get; set; } = Array.Empty<FinishedMedicineReportItemViewModel>();
    public IReadOnlyList<SupplierViewModel> Suppliers { get; set; } = Array.Empty<SupplierViewModel>();
}
