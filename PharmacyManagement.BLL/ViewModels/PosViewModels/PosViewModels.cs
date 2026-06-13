using PharmacyManagement.BLL.ViewModels.CustomerViewModels;
using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.BLL.ViewModels.PosViewModels;

public class PosBatchOptionViewModel
{
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int QuantityInStock { get; set; }
    public decimal SellingPrice { get; set; }
}

public class MedicineSaleLookupViewModel
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string MedicineForm { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string SaleUnit { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
    public int UnitsPerPurchaseUnit { get; set; }
    public int QuantityInStock { get; set; }
    public decimal SellingPrice { get; set; }
    public string UnitInfo => $"1 {PurchaseUnit} = {UnitsPerPurchaseUnit} {SaleUnit}(s)";
    public List<PosBatchOptionViewModel> Batches { get; set; } = new();
}

public class PosCartLineViewModel
{
    public int MedicineId { get; set; }
    public int? BatchId { get; set; }
    public string Dose { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string SaleUnit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int AvailableStock { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Subtotal => Math.Max(0, Quantity * UnitPrice - Discount);
}

public class PosCheckoutViewModel
{
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerPhone { get; set; }

    public string? DoctorName { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.Today;

    public int SaleType { get; set; } // 0 = Cash, 1 = Credit
    public decimal PaidAmount { get; set; }

    public List<PosCartLineViewModel> Items { get; set; } = new();
}

public class PosPageViewModel
{
    public IReadOnlyList<CustomerViewModel> Customers { get; set; } = Array.Empty<CustomerViewModel>();
}

public class PosCheckoutResultViewModel
{
    public int InvoiceId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}
