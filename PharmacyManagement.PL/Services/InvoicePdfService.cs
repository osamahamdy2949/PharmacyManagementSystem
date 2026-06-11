using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.InvoiceItemViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PharmacyManagement.PL.Services;

public class InvoicePdfService
{
    private readonly IPurchaseService _purchaseService;
    private readonly ISalesService _salesService;
    private readonly IReportService _reportService;

    public InvoicePdfService(IPurchaseService purchaseService, ISalesService salesService, IReportService reportService)
    {
        _purchaseService = purchaseService;
        _salesService = salesService;
        _reportService = reportService;
    }

    public async Task<byte[]?> GeneratePurchaseReceiptAsync(int invoiceId)
    {
        var invoice = await _purchaseService.GetByIdAsync(invoiceId);
        return invoice == null ? null : BuildInvoicePdf("Purchase Receipt", invoice.SupplierName ?? "", invoice.InvoiceDate, invoice.Items, invoice.TotalAmount, true);
    }

    public async Task<byte[]?> GenerateSalesReceiptAsync(int invoiceId)
    {
        var invoice = await _salesService.GetByIdAsync(invoiceId);
        return invoice == null ? null : BuildInvoicePdf("Sales Receipt", invoice.CustomerName ?? "", invoice.InvoiceDate, invoice.Items, invoice.TotalAmount, false);
    }

    public async Task<byte[]?> GenerateFinishedMedicinesReportAsync()
    {
        var items = await _reportService.GetFinishedMedicinesReportAsync();
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Margin(40);
                page.Header().Text("Finished Medicines Report").FontSize(18).Bold();
                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(60); c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn();
                        c.RelativeColumn(); c.ConstantColumn(70);
                    });
                    table.Header(h =>
                    {
                        foreach (var t in new[] { "Serial", "Name", "Form", "Purchase Unit", "Price/Unit", "Expiry" })
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(t).Bold();
                    });
                    foreach (var m in items)
                    {
                        table.Cell().Padding(3).Text(m.SerialNumber);
                        table.Cell().Padding(3).Text(m.TradeName);
                        table.Cell().Padding(3).Text(m.MedicineForm);
                        table.Cell().Padding(3).Text($"{m.PurchaseUnit} ({m.UnitInfo})");
                        table.Cell().Padding(3).Text(CurrencyHelper.FormatEgp(m.PurchasePricePerPurchaseUnit));
                        table.Cell().Padding(3).Text(DateDisplayHelper.FormatDate(m.ExpiryDate));
                    }
                });
            });
        }).GeneratePdf();
    }

    private static byte[] BuildInvoicePdf(string title, string partyName, DateTime date, List<InvoiceItemViewModel> items, decimal total, bool isPurchase)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Margin(40);
                page.Header().Column(col =>
                {
                    col.Item().Text("Pharmacy Management System").FontSize(14).Bold();
                    col.Item().Text(title).FontSize(18).FontColor(Colors.Teal.Darken2);
                    col.Item().Text($"{(isPurchase ? "Supplier" : "Customer")}: {partyName}");
                    col.Item().Text($"Date: {DateDisplayHelper.FormatDate(date)}");
                });
                page.Content().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); c.ConstantColumn(50); c.ConstantColumn(70);
                        c.ConstantColumn(80); c.ConstantColumn(80);
                    });
                    table.Header(h =>
                    {
                        foreach (var t in new[] { "Medicine", "Qty", "Unit", "Price", "Subtotal" })
                            h.Cell().Background(Colors.Teal.Medium).Padding(5).Text(t).FontColor(Colors.White);
                    });
                    foreach (var item in items)
                    {
                        table.Cell().Padding(4).Text($"{item.MedicineName} ({item.SerialNumber})");
                        table.Cell().Padding(4).Text(item.Quantity.ToString());
                        table.Cell().Padding(4).Text(item.UnitLabel ?? "");
                        table.Cell().Padding(4).Text(CurrencyHelper.FormatEgp(item.UnitPrice));
                        table.Cell().Padding(4).Text(CurrencyHelper.FormatEgp(item.Subtotal));
                    }
                });
                page.Footer().AlignRight().Text($"Total: {CurrencyHelper.FormatEgp(total)}").FontSize(14).Bold();
            });
        }).GeneratePdf();
    }
}
