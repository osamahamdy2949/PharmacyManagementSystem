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
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(9));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item().Text("PHARMACY MANAGEMENT SYSTEM").FontSize(9).FontColor("#6366F1").Bold().LetterSpacing(0.05f);
                            inner.Item().Text("Finished / Out of Stock Medicines").FontSize(18).Bold().FontColor("#0F172A");
                            inner.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy, HH:mm}").FontSize(8).FontColor("#64748B");
                        });
                    });
                    col.Item().PaddingTop(8).PaddingBottom(14).LineHorizontal(2).LineColor("#6366F1");
                });

                // Content Table
                page.Content().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(55);
                        c.RelativeColumn(2.5f);
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.RelativeColumn();
                        c.ConstantColumn(65);
                    });

                    // Table header
                    table.Header(h =>
                    {
                        var headerBg = "#4F46E5";
                        foreach (var t in new[] { "Serial", "Trade Name", "Form", "Purchase Unit", "Price / Unit", "Expiry" })
                            h.Cell().Background(headerBg).PaddingVertical(7).PaddingHorizontal(5)
                              .Text(t).FontColor(Colors.White).Bold().FontSize(8.5f);
                    });

                    // Rows
                    var rowIndex = 0;
                    foreach (var m in items)
                    {
                        var bg = rowIndex++ % 2 == 0 ? "#FFFFFF" : "#F8FAFC";
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5).Text(m.SerialNumber).FontSize(8);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5).Text(m.TradeName).Bold().FontSize(8);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5).Text(m.MedicineForm).FontSize(8);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5).Text($"{m.PurchaseUnit} ({m.UnitInfo})").FontSize(8);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5).Text(CurrencyHelper.FormatEgp(m.PurchasePricePerPurchaseUnit)).FontSize(8);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5).Text(DateDisplayHelper.FormatDate(m.ExpiryDate)).FontSize(8);
                    }
                });

                // Footer
                page.Footer().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text($"Total items: {items.Count}").FontSize(8).FontColor("#64748B");
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.Span("Page ").FontSize(8).FontColor("#64748B");
                        t.CurrentPageNumber().FontSize(8).FontColor("#64748B");
                        t.Span(" of ").FontSize(8).FontColor("#64748B");
                        t.TotalPages().FontSize(8).FontColor("#64748B");
                    });
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
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(9));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item().Text("PHARMACY MANAGEMENT SYSTEM").FontSize(8).FontColor("#6366F1").Bold().LetterSpacing(0.05f);
                            inner.Item().Text(title).FontSize(20).Bold().FontColor("#0F172A");
                            inner.Item().PaddingTop(4).Row(r2 =>
                            {
                                r2.RelativeItem().Text($"{(isPurchase ? "Supplier" : "Customer")}: {partyName}").FontSize(9).FontColor("#374151");
                                r2.RelativeItem().AlignRight().Text($"Date: {DateDisplayHelper.FormatDate(date)}").FontSize(9).FontColor("#374151");
                            });
                        });
                    });
                    col.Item().PaddingTop(10).PaddingBottom(14).LineHorizontal(2).LineColor("#6366F1");
                });

                // Items Table
                page.Content().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2.5f);
                        c.ConstantColumn(45);
                        c.ConstantColumn(65);
                        c.ConstantColumn(75);
                        c.ConstantColumn(80);
                    });

                    // Table header row
                    var headerBg = isPurchase ? "#4F46E5" : "#059669";
                    table.Header(h =>
                    {
                        foreach (var t in new[] { "Medicine", "Qty", "Unit", "Unit Price", "Subtotal" })
                            h.Cell().Background(headerBg).PaddingVertical(7).PaddingHorizontal(5)
                              .Text(t).FontColor(Colors.White).Bold().FontSize(8.5f);
                    });

                    // Rows with alternating background
                    var rowIndex = 0;
                    foreach (var item in items)
                    {
                        var bg = rowIndex++ % 2 == 0 ? "#FFFFFF" : "#F8FAFC";
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5)
                            .Text($"{item.MedicineName} ({item.SerialNumber})").Bold().FontSize(8);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5)
                            .Text(item.Quantity.ToString()).FontSize(8);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5)
                            .Text(item.UnitLabel ?? "").FontSize(8);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5)
                            .Text(CurrencyHelper.FormatEgp(item.UnitPrice)).FontSize(8);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#E2E8F0").PaddingVertical(5).PaddingHorizontal(5)
                            .Text(CurrencyHelper.FormatEgp(item.Subtotal)).FontSize(8);
                    }

                    // Total row
                    table.Cell().ColumnSpan(4).Background("#F1F5F9").PaddingVertical(6).PaddingHorizontal(5)
                        .AlignRight().Text("TOTAL").Bold().FontSize(9).FontColor("#0F172A");
                    table.Cell().Background("#EEF2FF").PaddingVertical(6).PaddingHorizontal(5)
                        .Text(CurrencyHelper.FormatEgp(total)).Bold().FontSize(9).FontColor("#4F46E5");
                });

                // Footer
                page.Footer().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text("Thank you for your business.").FontSize(8).FontColor("#94A3B8").Italic();
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.Span("Page ").FontSize(8).FontColor("#94A3B8");
                        t.CurrentPageNumber().FontSize(8).FontColor("#94A3B8");
                        t.Span(" of ").FontSize(8).FontColor("#94A3B8");
                        t.TotalPages().FontSize(8).FontColor("#94A3B8");
                    });
                });
            });
        }).GeneratePdf();
    }
}
