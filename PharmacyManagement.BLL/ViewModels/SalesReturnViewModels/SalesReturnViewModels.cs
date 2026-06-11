using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.BLL.ViewModels.SalesReturnViewModels;

public class SalesReturnViewModel
{
    public int Id { get; set; }
    public int SalesInvoiceId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? Reason { get; set; }
    public decimal TotalAmount { get; set; }
    public List<SalesReturnItemViewModel> Items { get; set; } = new();
}

public class SalesReturnItemViewModel
{
    public int MedicineBatchId { get; set; }
    public int MedicineId { get; set; }
    public string? MedicineName { get; set; }
    public string? BatchNumber { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

public class CreateSalesReturnViewModel
{
    [Required]
    public int SalesInvoiceId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime ReturnDate { get; set; } = DateTime.Today;

    [MaxLength(500)]
    public string? Reason { get; set; }

    public List<SalesReturnItemViewModel> Items { get; set; } = new();
}

public class SalesReturnListItemViewModel
{
    public int Id { get; set; }
    public int SalesInvoiceId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime ReturnDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Reason { get; set; }
}
