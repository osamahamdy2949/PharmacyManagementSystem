using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.DAL.SeedingData;

public static class PharmacyDbSeeder
{
    public static async Task SeedAsync(
        PharmacyDbContext context,
        ILogger logger)
    {
        try
        {
            logger.LogInformation("Starting Pharmacy database seeding...");

            if (await context.Categories.AnyAsync())
            {
                logger.LogInformation("Database already contains seed data. Skipping pharmacy seeding.");
                return;
            }

            logger.LogInformation("Seeding Categories...");

            var categories = new List<Category>
            {
                new() { Name = "Antibiotics", Description = "Antibacterial medicines" },
                new() { Name = "Pain Relief", Description = "Analgesics and anti-inflammatory" },
                new() { Name = "Vitamins", Description = "Supplements and vitamins" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            logger.LogInformation("Categories seeded successfully.");

            logger.LogInformation("Seeding Medicines...");

            var medicines = new List<Medicine>
            {
                new() { SerialNumber = "MED-001", TradeName = "Amoxil", ScientificName = "Amoxicillin", MedicineForm = MedicineForm.Capsules, PurchaseUnit = PurchaseUnit.Box, SaleUnit = SaleUnit.Strip, UnitsPerPurchaseUnit = 2, Description = "Broad-spectrum antibiotic", PurchasePrice = 55.00m, SellingPrice = 8.00m, Manufacturer = "GSK", Barcode = "8901234567001", StrengthValue = 500m, StrengthUnit = "mg", MinStockLevel = 10, IsActive = true, CategoryId = categories[0].Id },

                new() { SerialNumber = "MED-002", TradeName = "Panadol", ScientificName = "Paracetamol", MedicineForm = MedicineForm.Tablets, PurchaseUnit = PurchaseUnit.Box, SaleUnit = SaleUnit.Strip, UnitsPerPurchaseUnit = 2, Description = "Pain relief", PurchasePrice = 20.00m, SellingPrice = 4.50m, Manufacturer = "GSK", Barcode = "8901234567002", StrengthValue = 500m, StrengthUnit = "mg", MinStockLevel = 10, IsActive = true, CategoryId = categories[1].Id },

                new() { SerialNumber = "MED-003", TradeName = "Vitamin C", ScientificName = "Ascorbic Acid", MedicineForm = MedicineForm.Syrup, PurchaseUnit = PurchaseUnit.Box, SaleUnit = SaleUnit.Bottle, UnitsPerPurchaseUnit = 1, Description = "Immune support", PurchasePrice = 30.00m, SellingPrice = 6.00m, Manufacturer = "NaturePlus", Barcode = "8901234567003", StrengthValue = 1000m, StrengthUnit = "mg", MinStockLevel = 15, IsActive = true, CategoryId = categories[2].Id },

                new() { SerialNumber = "MED-004", TradeName = "Ibuprofen", ScientificName = "Ibuprofen", MedicineForm = MedicineForm.Tablets, PurchaseUnit = PurchaseUnit.Box, SaleUnit = SaleUnit.Strip, UnitsPerPurchaseUnit = 3, Description = "NSAID", PurchasePrice = 40.00m, SellingPrice = 7.50m, Manufacturer = "Pfizer", Barcode = "8901234567004", StrengthValue = 400m, StrengthUnit = "mg", MinStockLevel = 10, IsActive = true, CategoryId = categories[1].Id }
            };

            context.Medicines.AddRange(medicines);
            await context.SaveChangesAsync();

            logger.LogInformation("Medicines seeded successfully.");

            var batches = new List<MedicineBatch>
            {
                CreateBatch(medicines[0], "BATCH-001A", DateTime.Today.AddMonths(18), 120),
                CreateBatch(medicines[1], "BATCH-002A", DateTime.Today.AddMonths(24), 8),
                CreateBatch(medicines[2], "BATCH-003A", DateTime.Today.AddDays(20), 50),
                CreateBatch(medicines[3], "BATCH-004A", DateTime.Today.AddMonths(12), 75)
            };

            context.MedicineBatches.AddRange(batches);
            await context.SaveChangesAsync();

            logger.LogInformation("Medicine batches seeded successfully.");

            var suppliers = new List<Supplier>
            {
                new() { Name = "MedSupply Co.", Email = "contact@medsupply.com", Phone = "01234567890", Address = "123 Health St", TaxRegNumber = "TRN-10001" },
                new() { Name = "PharmaGlobal", Email = "sales@pharmaglobal.com", Phone = "01098765432", Address = "45 Industrial Ave", TaxRegNumber = "TRN-10002" }
            };

            context.Suppliers.AddRange(suppliers);
            await context.SaveChangesAsync();

            logger.LogInformation("Suppliers seeded successfully.");

            var customers = new List<Customer>
            {
                new() { Name = "Ahmed Hassan", Phone = "01112223334" },
                new() { Name = "Sara Mohamed", Phone = "05556667778" }
            };

            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();

            logger.LogInformation("Customers seeded successfully.");

            var purchaseSub = 50m * 5.50m + 30m * 3.00m;

            var purchaseInvoice = new PurchaseInvoice
            {
                InvoiceDate = DateTime.Today.AddDays(-5),
                SupplierId = suppliers[0].Id,
                SubTotal = purchaseSub,
                VatAmount = 0,
                TotalAmount = purchaseSub
            };

            context.PurchaseInvoices.Add(purchaseInvoice);
            await context.SaveChangesAsync();

            var batchForMed0 = CreateBatch(medicines[0], "BATCH-001B", DateTime.Today.AddMonths(18), 50);
            var batchForMed2 = CreateBatch(medicines[2], "BATCH-003B", DateTime.Today.AddMonths(12), 30);

            context.MedicineBatches.AddRange(batchForMed0, batchForMed2);
            await context.SaveChangesAsync();

            context.PurchaseInvoiceItems.AddRange(
                new PurchaseInvoiceItem
                {
                    PurchaseInvoiceId = purchaseInvoice.Id,
                    MedicineId = medicines[0].Id,
                    MedicineBatchId = batchForMed0.Id,
                    Quantity = 50,
                    UnitPrice = 5.50m,
                    BatchNumber = "BATCH-001B",
                    ExpiryDate = DateTime.Today.AddMonths(18),
                    SellingPrice = 8.00m
                },
                new PurchaseInvoiceItem
                {
                    PurchaseInvoiceId = purchaseInvoice.Id,
                    MedicineId = medicines[2].Id,
                    MedicineBatchId = batchForMed2.Id,
                    Quantity = 30,
                    UnitPrice = 3.00m,
                    BatchNumber = "BATCH-003B",
                    ExpiryDate = DateTime.Today.AddMonths(12),
                    SellingPrice = 6.00m
                });

            var salesSub = 2m * 4.50m + 5m * 7.50m;

            var salesInvoice = new SalesInvoice
            {
                InvoiceDate = DateTime.Today.AddDays(-2),
                CustomerId = customers[0].Id,
                SubTotal = salesSub,
                VatAmount = 0,
                TotalAmount = salesSub
            };

            context.SalesInvoices.Add(salesInvoice);
            await context.SaveChangesAsync();

            batches[1].CurrentQuantity -= 2;
            batches[3].CurrentQuantity -= 5;

            context.SalesInvoiceItems.AddRange(
                new SalesInvoiceItem
                {
                    SalesInvoiceId = salesInvoice.Id,
                    MedicineId = medicines[1].Id,
                    MedicineBatchId = batches[1].Id,
                    Quantity = 2,
                    UnitPrice = 4.50m
                },
                new SalesInvoiceItem
                {
                    SalesInvoiceId = salesInvoice.Id,
                    MedicineId = medicines[3].Id,
                    MedicineBatchId = batches[3].Id,
                    Quantity = 5,
                    UnitPrice = 7.50m
                });

            customers[0].TotalSpent = salesInvoice.TotalAmount;

            await context.SaveChangesAsync();

            logger.LogInformation("Pharmacy database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the pharmacy database.");
            throw;
        }
    }

    private static MedicineBatch CreateBatch(Medicine medicine, string batchNumber, DateTime expiry, int qty) => new()
    {
        MedicineId = medicine.Id,
        Sku = medicine.SerialNumber,
        Dose = $"{medicine.StrengthValue} {medicine.StrengthUnit}",
        BatchNumber = batchNumber,
        Barcode = medicine.Barcode,
        ExpiryDate = expiry,
        PurchaseUnit = medicine.PurchaseUnit,
        SaleUnit = medicine.SaleUnit,
        UnitsPerPurchaseUnit = medicine.UnitsPerPurchaseUnit,
        PurchasePrice = medicine.PurchasePrice,
        SellingPrice = medicine.SellingPrice,
        MinStockLevel = medicine.MinStockLevel,
        IsActive = true,
        CurrentQuantity = qty
    };
}