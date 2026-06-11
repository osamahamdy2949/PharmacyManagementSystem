namespace PharmacyManagement.BLL.ViewModels.ReportViewModels;

public class FinishedMedicineReportItemViewModel
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string MedicineForm { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
    public string SaleUnit { get; set; } = string.Empty;
    public int UnitsPerPurchaseUnit { get; set; }
    public decimal PurchasePricePerPurchaseUnit { get; set; }
    public string UnitInfo => $"1 {PurchaseUnit} = {UnitsPerPurchaseUnit} {SaleUnit}(s)";
    public DateTime ExpiryDate { get; set; }
}

public class InventoryReportItemViewModel
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public decimal SellingPrice { get; set; }
    public DateTime ExpiryDate { get; set; }
}

public class SalesReportItemViewModel
{
    public int InvoiceId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class PurchaseReportItemViewModel
{
    public int InvoiceId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class ExpiredMedicineReportItemViewModel
{
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal LossValue { get; set; }
}

public class TopSellingMedicineViewModel
{
    public string MedicineName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
}

public class SalesByCategoryViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
}

public class ProfitReportViewModel
{
    public decimal TotalSales { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal Profit => TotalSales - TotalPurchases;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class BatchInventoryItemViewModel
{
    public int BatchId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public string MedicineForm { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public int StockByPurchaseUnit { get; set; }
    public int StockBySellingUnit { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
}

public enum InventoryFilter
{
    All,
    LowStock,
    NearExpiry,
    Expired,
    OutOfStock
}

public enum ReportPeriod
{
    Daily,
    Weekly,
    Monthly,
    Custom
}
