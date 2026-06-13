using PharmacyManagement.BLL.ViewModels.InvoiceItemViewModels;
using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.BLL.ViewModels.SalesInvoiceViewModels;

public class SalesInvoiceViewModel
{
    public int Id { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime InvoiceDate { get; set; }

    public decimal SubTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }
    public string? DoctorName { get; set; }
    
    public string SaleType { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    
    public string? CreatedByUserName { get; set; }
    public List<InvoiceItemViewModel> Items { get; set; } = new();
}

public class CreateSalesInvoiceViewModel
{
    [Required]
    public int CustomerId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    public List<InvoiceItemViewModel> Items { get; set; } = new() { new(), new(), new() };
}
