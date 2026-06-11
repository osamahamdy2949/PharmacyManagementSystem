using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.BLL.ViewModels.InvoiceItemViewModels;

public class InvoiceItemViewModel
{
    public int MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public string? SerialNumber { get; set; }
    public string? UnitLabel { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }
}
