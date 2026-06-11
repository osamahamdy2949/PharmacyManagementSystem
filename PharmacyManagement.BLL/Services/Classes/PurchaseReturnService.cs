using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.PurchaseReturnViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockService _stockService;
    private readonly ICurrentUserService _currentUser;

    public PurchaseReturnService(IUnitOfWork unitOfWork, IStockService stockService, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _stockService = stockService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PurchaseReturnListItemViewModel>> GetAllAsync()
    {
        return await _unitOfWork.GetRepository<PurchaseReturn>().Query()
            .Include(r => r.Supplier)
            .OrderByDescending(r => r.ReturnDate)
            .Select(r => new PurchaseReturnListItemViewModel
            {
                Id = r.Id,
                PurchaseInvoiceId = r.PurchaseInvoiceId,
                SupplierName = r.Supplier.Name,
                ReturnDate = r.ReturnDate,
                Reason = r.Reason,
                TotalAmount = r.TotalAmount
            }).ToListAsync();
    }

    public async Task<PurchaseReturnViewModel?> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.GetRepository<PurchaseReturn>().Query()
            .Include(r => r.Supplier)
            .Include(r => r.Items).ThenInclude(i => i.Medicine)
            .Include(r => r.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (item == null) return null;

        return new PurchaseReturnViewModel
        {
            Id = item.Id,
            PurchaseInvoiceId = item.PurchaseInvoiceId,
            SupplierName = item.Supplier.Name,
            ReturnDate = item.ReturnDate,
            Reason = item.Reason,
            Notes = item.Notes,
            TotalAmount = item.TotalAmount,
            Items = item.Items.Select(i => new PurchaseReturnItemViewModel
            {
                MedicineBatchId = i.MedicineBatchId,
                MedicineId = i.MedicineId,
                MedicineName = i.Medicine.TradeName,
                BatchNumber = i.MedicineBatch.BatchNumber,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }

    public async Task<CreatePurchaseReturnViewModel?> GetCreateModelFromInvoiceAsync(int invoiceId)
    {
        var invoice = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .Include(p => p.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(p => p.Id == invoiceId);

        if (invoice == null) return null;

        var items = new List<PurchaseReturnItemViewModel>();
        foreach (var i in invoice.Items.Where(i => i.MedicineBatchId.HasValue))
        {
            var maxBoxes = await _unitOfWork.GetRepository<MedicineBatch>().Query()
                .Where(b => b.MedicineId == i.MedicineId && b.BatchNumber == i.BatchNumber && b.CurrentQuantity > 0)
                .CountAsync();

            items.Add(new PurchaseReturnItemViewModel
            {
                MedicineBatchId = i.MedicineBatchId!.Value,
                MedicineId = i.MedicineId,
                MedicineName = i.Medicine.TradeName,
                BatchNumber = i.BatchNumber,
                Quantity = 0,
                MaxQuantity = maxBoxes,
                UnitPrice = i.UnitPrice
            });
        }

        return new CreatePurchaseReturnViewModel
        {
            PurchaseInvoiceId = invoice.Id,
            Items = items
        };
    }

    public async Task<ServiceResult<int>> CreateAsync(CreatePurchaseReturnViewModel model)
    {
        var invoice = await _unitOfWork.GetRepository<PurchaseInvoice>().Query()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == model.PurchaseInvoiceId);

        if (invoice == null)
            return ServiceResult<int>.Fail("Purchase invoice not found.");

        var returnLines = model.Items.Where(i => i.Quantity > 0).ToList();
        if (returnLines.Count == 0)
            return ServiceResult<int>.Fail("At least one returned item is required.");

        var returnItems = returnLines.Select(l => new PurchaseReturnItem
        {
            MedicineBatchId = l.MedicineBatchId,
            MedicineId = l.MedicineId,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            Reason = model.Reason.ToString()
        }).ToList();

        var purchaseReturn = new PurchaseReturn
        {
            PurchaseInvoiceId = invoice.Id,
            SupplierId = invoice.SupplierId,
            ReturnDate = model.ReturnDate.Date,
            Reason = model.Reason,
            Notes = model.Notes,
            TotalAmount = returnItems.Sum(i => i.Quantity * i.UnitPrice),
            CreatedByUserId = _currentUser.UserId,
            Items = returnItems
        };

        await using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
        try
        {
            _unitOfWork.GetRepository<PurchaseReturn>().Add(purchaseReturn);
            await _unitOfWork.SaveChangesAsync();

            foreach (var line in returnItems)
            {
                var originalBatch = await _unitOfWork.GetRepository<MedicineBatch>().GetByIdAsync(line.MedicineBatchId);
                if (originalBatch == null) continue;

                var clonesToReturn = await _unitOfWork.GetRepository<MedicineBatch>().Query()
                    .Where(b => b.MedicineId == line.MedicineId && b.BatchNumber == originalBatch.BatchNumber && b.CurrentQuantity > 0)
                    .Take(line.Quantity)
                    .ToListAsync();

                if (clonesToReturn.Count < line.Quantity)
                    throw new InvalidOperationException($"Insufficient stock in batch {originalBatch.BatchNumber}.");

                foreach (var clone in clonesToReturn)
                {
                    await _stockService.DeductFromBatchAsync(clone.Id, clone.CurrentQuantity, purchaseReturn.Id);
                }
            }

            await transaction.CommitAsync();
            return ServiceResult<int>.Ok(purchaseReturn.Id);
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult<int>.Fail(ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
