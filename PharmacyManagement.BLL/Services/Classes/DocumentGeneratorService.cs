using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.ReportViewModels;

namespace PharmacyManagement.BLL.Services.Classes;

public class DocumentGeneratorService : IDocumentGeneratorService
{
    public DocumentGeneratorService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateSalesExcelReport(IReadOnlyList<SalesReportItemViewModel> sales)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sales Report");

        // Headers
        ws.Cell(1, 1).Value = "Invoice #";
        ws.Cell(1, 2).Value = "Date";
        ws.Cell(1, 3).Value = "Customer";
        ws.Cell(1, 4).Value = "Cashier";
        ws.Cell(1, 5).Value = "Total Amount";
        ws.Cell(1, 6).Value = "Sale Type";

        var headerRange = ws.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        int row = 2;
        foreach (var sale in sales)
        {
            ws.Cell(row, 1).Value = sale.InvoiceId;
            ws.Cell(row, 2).Value = sale.InvoiceDate.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 3).Value = sale.CustomerName;
            ws.Cell(row, 4).Value = sale.CashierName;
            ws.Cell(row, 5).Value = sale.TotalAmount;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = sale.SaleType;
            row++;
        }

        // Auto-fit columns
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateSalesPdfReport(IReadOnlyList<SalesReportItemViewModel> sales, string title)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(compose => ComposeHeader(compose, title));
                page.Content().Element(compose => ComposeSalesTable(compose, sales));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }
    
    public byte[] GenerateInventoryExcelReport(IReadOnlyList<InventoryReportItemViewModel> inventory)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Inventory Report");

        // Headers
        ws.Cell(1, 1).Value = "Medicine";
        ws.Cell(1, 2).Value = "Category";
        ws.Cell(1, 3).Value = "Current Stock";
        ws.Cell(1, 4).Value = "Unit";
        ws.Cell(1, 5).Value = "Purchase Price";
        ws.Cell(1, 6).Value = "Selling Price";
        ws.Cell(1, 7).Value = "Total Value (Cost)";

        var headerRange = ws.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        int row = 2;
        foreach (var item in inventory)
        {
            ws.Cell(row, 1).Value = item.MedicineName;
            ws.Cell(row, 2).Value = item.CategoryName;
            ws.Cell(row, 3).Value = item.CurrentStock;
            ws.Cell(row, 4).Value = item.Unit;
            ws.Cell(row, 5).Value = item.PurchasePrice;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = item.SellingPrice;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Value = item.TotalValueCost;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    
    public byte[] GenerateInventoryPdfReport(IReadOnlyList<InventoryReportItemViewModel> inventory, string title)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(compose => ComposeHeader(compose, title));
                page.Content().Element(compose => ComposeInventoryTable(compose, inventory));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, string title)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Pharmacy Management System").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text(title).FontSize(14).FontColor(Colors.Grey.Darken2);
                column.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComposeSalesTable(IContainer container, IReadOnlyList<SalesReportItemViewModel> sales)
    {
        container.PaddingTop(1, Unit.Centimetre).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(50); // ID
                columns.RelativeColumn(2);  // Date
                columns.RelativeColumn(3);  // Customer
                columns.RelativeColumn(2);  // Cashier
                columns.RelativeColumn(2);  // Sale Type
                columns.RelativeColumn(2);  // Total
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("ID");
                header.Cell().Element(CellStyle).Text("Date");
                header.Cell().Element(CellStyle).Text("Customer");
                header.Cell().Element(CellStyle).Text("Cashier");
                header.Cell().Element(CellStyle).Text("Type");
                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            foreach (var sale in sales)
            {
                table.Cell().Element(CellStyle).Text(sale.InvoiceId.ToString());
                table.Cell().Element(CellStyle).Text(sale.InvoiceDate.ToString("yyyy-MM-dd"));
                table.Cell().Element(CellStyle).Text(sale.CustomerName);
                table.Cell().Element(CellStyle).Text(sale.CashierName);
                table.Cell().Element(CellStyle).Text(sale.SaleType);
                table.Cell().Element(CellStyle).AlignRight().Text($"{sale.TotalAmount:N2}");

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            }
            
            // Grand Total
            table.Cell().ColumnSpan(5).Element(TotalStyle).AlignRight().Text("Grand Total: ");
            table.Cell().Element(TotalStyle).AlignRight().Text($"{sales.Sum(s => s.TotalAmount):N2}");
            
            static IContainer TotalStyle(IContainer container)
            {
                return container.PaddingTop(10).DefaultTextStyle(x => x.SemiBold().FontSize(11));
            }
        });
    }
    
    private void ComposeInventoryTable(IContainer container, IReadOnlyList<InventoryReportItemViewModel> inventory)
    {
        container.PaddingTop(1, Unit.Centimetre).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);  // Medicine
                columns.RelativeColumn(2);  // Category
                columns.ConstantColumn(60); // Stock
                columns.RelativeColumn(2);  // Purchase Price
                columns.RelativeColumn(2);  // Total Value
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Medicine");
                header.Cell().Element(CellStyle).Text("Category");
                header.Cell().Element(CellStyle).AlignRight().Text("Stock");
                header.Cell().Element(CellStyle).AlignRight().Text("Cost Price");
                header.Cell().Element(CellStyle).AlignRight().Text("Total Value");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            foreach (var item in inventory)
            {
                table.Cell().Element(CellStyle).Text(item.MedicineName);
                table.Cell().Element(CellStyle).Text(item.CategoryName);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.CurrentStock} {item.Unit}");
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.PurchasePrice:N2}");
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalValueCost:N2}");

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            }
            
            // Grand Total
            table.Cell().ColumnSpan(4).Element(TotalStyle).AlignRight().Text("Total Inventory Value (Cost): ");
            table.Cell().Element(TotalStyle).AlignRight().Text($"{inventory.Sum(s => s.TotalValueCost):N2}");
            
            static IContainer TotalStyle(IContainer container)
            {
                return container.PaddingTop(10).DefaultTextStyle(x => x.SemiBold().FontSize(11));
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
            x.Span(" of ");
            x.TotalPages();
        });
    }
}
