using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.Validators;
using PharmacyManagement.BLL.ViewModels.PurchaseInvoiceViewModels;
using PharmacyManagement.BLL.ViewModels.RestockViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class PurchaseService : IPurchaseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStockService _stockService;
    private readonly IValidator<CreatePurchaseInvoiceViewModel> _createValidator;
    private readonly ICurrentUserService _currentUser;

    public PurchaseService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IStockService stockService,
        IValidator<CreatePurchaseInvoiceViewModel> createValidator,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _stockService = stockService;
        _createValidator = createValidator;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PurchaseInvoiceViewModel>> GetAllAsync()
    {
        var items = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .OrderByDescending(p => p.InvoiceDate)
            .ToListAsync();
        return _mapper.Map<IReadOnlyList<PurchaseInvoiceViewModel>>(items);
    }

    public async Task<PurchaseInvoiceViewModel?> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.Id == id);
        return item == null ? null : _mapper.Map<PurchaseInvoiceViewModel>(item);
    }

    public async Task<string> GetNextBatchNumberAsync()
    {
        var existing = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .Select(b => b.BatchNumber)
            .ToListAsync();

        var max = 0;
        foreach (var bn in existing)
        {
            if (bn.StartsWith("B-", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(bn[2..], out var num) && num > max)
                max = num;
        }

        return $"B-{(max + 1):D2}";
    }

    public async Task<IReadOnlyList<string>> GetExistingBatchNumbersAsync() =>
        await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .AsNoTracking()
            .Select(b => b.BatchNumber)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync();

    public async Task<IReadOnlyList<PendingBatchViewModel>> GetPendingBatchesAsync()
    {
        var pendingBatches = _unitOfWork.GetRepository<MedicineBatch>().Query()
            .AsNoTracking()
            .Where(b => !b.IsActive && b.CurrentQuantity > 0);

        var invoiceItems = _unitOfWork.GetRepository<PurchaseInvoiceItem>().Query()
            .AsNoTracking();

        return await (
            from batch in pendingBatches
            join item in invoiceItems on batch.Id equals item.MedicineBatchId into batchItems
            from invoiceItem in batchItems.Take(1).DefaultIfEmpty()
            orderby batch.Medicine.TradeName
            select new PendingBatchViewModel
            {
                BatchId = batch.Id,
                PurchaseInvoiceId = invoiceItem == null ? 0 : invoiceItem.PurchaseInvoiceId,
                MedicineName = batch.Medicine.TradeName,
                Sku = batch.Sku,
                Dose = batch.Dose,
                BatchNumber = batch.BatchNumber,
                SupplierName = invoiceItem == null ? "" : invoiceItem.PurchaseInvoice.Supplier.Name,
                PendingQuantity = batch.CurrentQuantity,
                PurchaseUnit = batch.Medicine.PurchaseUnit.ToString()
            })
            .ToListAsync();
    }

    public async Task<ActivateBatchViewModel?> GetActivateBatchModelAsync(int batchId)
    {
        var batch = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .Include(b => b.Medicine)
            .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsActive);

        if (batch == null) return null;

        var medicine = batch.Medicine;
        return new ActivateBatchViewModel
        {
            BatchId = batch.Id,
            MedicineName = medicine.TradeName,
            Sku = batch.Sku,
            Dose = batch.Dose,
            BatchNumber = batch.BatchNumber,
            PendingQuantity = batch.CurrentQuantity,
            Barcodes = Enumerable.Repeat(string.Empty, batch.CurrentQuantity).ToList(),
            // Leave units and prices empty/0 to force user to enter them explicitly
            PurchaseUnit = null,
            SaleUnit = null,
            UnitsPerPurchaseUnit = 0,
            PurchasePrice = 0,
            SellingPrice = 0,
            MinStockLevel = medicine.MinStockLevel > 0 ? medicine.MinStockLevel : 10
        };
    }

    public async Task<ServiceResult> ActivateBatchAsync(ActivateBatchViewModel model)
    {
        var batch = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .Include(b => b.Medicine)
            .FirstOrDefaultAsync(b => b.Id == model.BatchId && !b.IsActive);

        if (batch == null)
            return ServiceResult.Fail("Pending batch not found or already activated.");

        var purchaseUnits = batch.CurrentQuantity;
        if (model.Barcodes == null || model.Barcodes.Count != purchaseUnits)
            return ServiceResult.Fail($"You must provide exactly {purchaseUnits} unique barcodes (one for each box).");

        var uniqueBarcodes = model.Barcodes.Select(b => b.Trim()).Where(b => !string.IsNullOrEmpty(b)).Distinct().ToList();
        if (uniqueBarcodes.Count != purchaseUnits)
            return ServiceResult.Fail("All provided barcodes must be unique and non-empty.");

        var existingBarcodes = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .Where(b => b.Barcode != null && uniqueBarcodes.Contains(b.Barcode))
            .Select(b => b.Barcode)
            .ToListAsync();

        if (existingBarcodes.Any())
            return ServiceResult.Fail($"The following barcodes already exist: {string.Join(", ", existingBarcodes)}");

        var saleUnitsPerBox = model.UnitsPerPurchaseUnit;

        // 1. Update the original batch (represents the first box)
        batch.Barcode = uniqueBarcodes[0];
        batch.ExpiryDate = model.ExpiryDate!.Value.Date;
        batch.ManufactureDate = model.ManufactureDate?.Date;
        batch.PurchaseUnit = model.PurchaseUnit!.Value;
        batch.SaleUnit = model.SaleUnit!.Value;
        batch.UnitsPerPurchaseUnit = model.UnitsPerPurchaseUnit;
        batch.PurchasePrice = model.PurchasePrice;
        batch.SellingPrice = model.SellingPrice;
        batch.MinStockLevel = model.MinStockLevel;
        batch.CurrentQuantity = saleUnitsPerBox;
        batch.IsActive = true;

        var batchesToInsert = new List<MedicineBatch>();
        
        // 2. Create clones for the remaining boxes
        for (int i = 1; i < purchaseUnits; i++)
        {
            var clone = new MedicineBatch
            {
                MedicineId = batch.MedicineId,
                Sku = batch.Sku,
                Dose = batch.Dose,
                BatchNumber = batch.BatchNumber, // Keep same batch number to group them
                Barcode = uniqueBarcodes[i],
                ExpiryDate = batch.ExpiryDate,
                ManufactureDate = batch.ManufactureDate,
                PurchaseUnit = batch.PurchaseUnit,
                SaleUnit = batch.SaleUnit,
                UnitsPerPurchaseUnit = batch.UnitsPerPurchaseUnit,
                PurchasePrice = batch.PurchasePrice,
                SellingPrice = batch.SellingPrice,
                MinStockLevel = batch.MinStockLevel,
                CurrentQuantity = saleUnitsPerBox,
                IsActive = true
            };
            batchesToInsert.Add(clone);
            _unitOfWork.GetRepository<MedicineBatch>().Add(clone);
        }

        var medicine = batch.Medicine;
        medicine.PurchaseUnit = model.PurchaseUnit!.Value;
        medicine.SaleUnit = model.SaleUnit!.Value;
        medicine.UnitsPerPurchaseUnit = model.UnitsPerPurchaseUnit;
        medicine.PurchasePrice = model.PurchasePrice;
        medicine.SellingPrice = model.SellingPrice;
        medicine.MinStockLevel = model.MinStockLevel;
        // Do NOT update medicine.Barcode with a box-specific serial to avoid unique constraint violations

        await _unitOfWork.SaveChangesAsync();

        // 3. Update stock transactions
        var invoiceId = await _unitOfWork.GetRepository<PurchaseInvoiceItem>().Query()
            .Where(i => i.MedicineBatchId == batch.Id)
            .Select(i => i.PurchaseInvoiceId)
            .FirstOrDefaultAsync();

        if (invoiceId > 0)
        {
            // Record transaction for the original batch
            await _stockService.RecordTransactionAsync(batch.MedicineId, saleUnitsPerBox, DAL.Data.Entities.Enums.StockTransactionType.Purchase, batch.Id, invoiceId);
            
            // Record transactions for the clones
            foreach (var clone in batchesToInsert)
            {
                await _stockService.RecordTransactionAsync(clone.MedicineId, saleUnitsPerBox, DAL.Data.Entities.Enums.StockTransactionType.Purchase, clone.Id, invoiceId);
            }
        }

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<int>> CreateAsync(CreatePurchaseInvoiceViewModel model)
    {
        var validation = await ValidationHelper.ValidateAsync(_createValidator, model);
        if (validation != null)
            return ServiceResult<int>.FromValidation(validation);

        if (!await _unitOfWork.GetRepository<Supplier>().AnyAsync(s => s.Id == model.SupplierId))
            return ServiceResult<int>.Fail("Supplier does not exist.");

        var activeLines = model.Items
            .Where(i => i.MedicineId > 0 && i.Quantity > 0 && !string.IsNullOrWhiteSpace(i.Dose))
            .ToList();

        var invoiceItems = new List<PurchaseInvoiceItem>();
        var subTotal = 0m;

        var existingBatches = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .Select(b => b.BatchNumber)
            .ToListAsync();
        var nextBatchBase = 0;
        foreach (var bn in existingBatches)
        {
            if (bn.StartsWith("B-", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(bn[2..], out var num) && num > nextBatchBase)
                nextBatchBase = num;
        }

        foreach (var line in activeLines)
        {
            var medicine = await _unitOfWork.GetRepository<Medicine>().Query()
                .FirstOrDefaultAsync(m => m.Id == line.MedicineId);

            if (medicine == null)
                return ServiceResult<int>.Fail($"Medicine id {line.MedicineId} not found.");

            nextBatchBase++;
            var batchNumber = $"B-{nextBatchBase:D2}";

            var unitPrice = medicine.PurchasePrice > 0 ? medicine.PurchasePrice : 0;
            subTotal += line.Quantity * unitPrice;

            var item = new PurchaseInvoiceItem
            {
                MedicineId = medicine.Id,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                BatchNumber = batchNumber,
                ExpiryDate = DateTime.Today.AddYears(1),
                SellingPrice = medicine.SellingPrice > 0 ? medicine.SellingPrice : 0
            };

            invoiceItems.Add(item);
        }

        var invoice = new PurchaseInvoice
        {
            SupplierId = model.SupplierId,
            InvoiceDate = model.InvoiceDate.Date,
            SubTotal = subTotal,
            VatAmount = 0,
            TotalAmount = subTotal,
            CreatedByUserId = _currentUser.UserId,
            Items = invoiceItems
        };

        await using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
        try
        {
            _unitOfWork.GetRepository<PurchaseInvoice>().Add(invoice);
            await _unitOfWork.SaveChangesAsync();

            for (var i = 0; i < activeLines.Count; i++)
            {
                var line = activeLines[i];
                var item = invoiceItems[i];
                var medicine = await _unitOfWork.GetRepository<Medicine>().Query()
                    .FirstAsync(m => m.Id == line.MedicineId);

                var batchNumber = item.BatchNumber;

                var batch = new MedicineBatch
                {
                    MedicineId = medicine.Id,
                    Sku = medicine.SerialNumber,
                    Dose = line.Dose.Trim(),
                    BatchNumber = batchNumber,
                    PurchaseUnit = medicine.PurchaseUnit,
                    SaleUnit = medicine.SaleUnit,
                    UnitsPerPurchaseUnit = Math.Max(1, medicine.UnitsPerPurchaseUnit),
                    CurrentQuantity = line.Quantity,
                    IsActive = false
                };

                var created = await _stockService.AddBatchStockAsync(batch, invoice.Id, recordTransaction: false);
                item.MedicineBatchId = created.Id;
            }

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return ServiceResult<int>.Ok(invoice.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ServiceResult<int>> RestockFinishedMedicineAsync(RestockFinishedMedicineRequest request)
    {
        if (request.SupplierId <= 0)
            return ServiceResult<int>.Fail("Supplier is required.");

        if (request.PurchaseQuantity <= 0)
            return ServiceResult<int>.Fail("Purchase quantity must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Dose))
            return ServiceResult<int>.Fail("Dose is required.");

        var medicine = await _unitOfWork.GetRepository<Medicine>().Query()
            .FirstOrDefaultAsync(m => m.Id == request.MedicineId);

        if (medicine == null)
            return ServiceResult<int>.Fail("Medicine not found.");

        var availableStock = await _stockService.GetAvailableStockAsync(medicine.Id);

        if (availableStock > 0)
            return ServiceResult<int>.Fail("Medicine is not out of stock. Restock is only for finished items.");

        if (!await _unitOfWork.GetRepository<Supplier>().AnyAsync(s => s.Id == request.SupplierId))
            return ServiceResult<int>.Fail("Supplier does not exist.");

        var batchNumber = string.IsNullOrWhiteSpace(request.BatchNumber)
            ? await GetNextBatchNumberAsync()
            : request.BatchNumber.Trim();

        var unitPrice = medicine.PurchasePrice > 0 ? medicine.PurchasePrice : 0;
        var lineTotal = request.PurchaseQuantity * unitPrice;

        var invoice = new PurchaseInvoice
        {
            SupplierId = request.SupplierId,
            InvoiceDate = DateTime.Now,
            SubTotal = lineTotal,
            VatAmount = 0,
            TotalAmount = lineTotal,
            CreatedByUserId = _currentUser.UserId,
            Items =
            [
                new PurchaseInvoiceItem
                {
                    MedicineId = medicine.Id,
                    Quantity = request.PurchaseQuantity,
                    UnitPrice = unitPrice,
                    BatchNumber = batchNumber,
                    ExpiryDate = DateTime.Today.AddYears(1),
                    SellingPrice = medicine.SellingPrice > 0 ? medicine.SellingPrice : 0
                }
            ]
        };

        _unitOfWork.GetRepository<PurchaseInvoice>().Add(invoice);
        await _unitOfWork.SaveChangesAsync();

        var batch = new MedicineBatch
        {
            MedicineId = medicine.Id,
            Sku = medicine.SerialNumber,
            Dose = request.Dose.Trim(),
            BatchNumber = batchNumber,
            PurchaseUnit = medicine.PurchaseUnit,
            SaleUnit = medicine.SaleUnit,
            UnitsPerPurchaseUnit = Math.Max(1, medicine.UnitsPerPurchaseUnit),
            CurrentQuantity = request.PurchaseQuantity,
            IsActive = false
        };

        var created = await _stockService.AddBatchStockAsync(batch, invoice.Id, recordTransaction: false);
        invoice.Items.First().MedicineBatchId = created.Id;
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<int>.Ok(invoice.Id);
    }

    public async Task<ServiceResult> EditPendingBatchQuantityAsync(int batchId, int newQuantity)
    {
        if (newQuantity <= 0)
            return ServiceResult.Fail("Quantity must be greater than 0.");

        var batch = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsActive);

        if (batch == null)
            return ServiceResult.Fail("Pending batch not found.");

        batch.CurrentQuantity = newQuantity;
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeletePendingBatchAsync(int batchId)
    {
        var batch = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsActive);

        if (batch == null)
            return ServiceResult.Fail("Pending batch not found.");

        _unitOfWork.GetRepository<MedicineBatch>().Remove(batch);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
