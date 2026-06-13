using PharmacyManagement.BLL.ViewModels.ReportViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface IDocumentGeneratorService
{
    byte[] GenerateSalesExcelReport(IReadOnlyList<SalesReportItemViewModel> sales);
    byte[] GenerateSalesPdfReport(IReadOnlyList<SalesReportItemViewModel> sales, string title);
    
    byte[] GenerateInventoryExcelReport(IReadOnlyList<InventoryReportItemViewModel> inventory);
    byte[] GenerateInventoryPdfReport(IReadOnlyList<InventoryReportItemViewModel> inventory, string title);
}
