using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.PurchaseReturnViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPurchaseReturnRepository _purchaseReturnRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IDataTransactionManager _transactionManager;
    private readonly IStockService _stockService;
    private readonly ICurrentUserService _currentUser;

    public PurchaseReturnService(
        IUnitOfWork unitOfWork,
        IPurchaseReturnRepository purchaseReturnRepository,
        IStockRepository stockRepository,
        IDataTransactionManager transactionManager,
        IStockService stockService,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _purchaseReturnRepository = purchaseReturnRepository;
        _stockRepository = stockRepository;
        _transactionManager = transactionManager;
        _stockService = stockService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PurchaseReturnListItemViewModel>> GetAllAsync()
    {
        var items = await _purchaseReturnRepository.GetAllWithSupplierAsync();
        return items.Select(r => new PurchaseReturnListItemViewModel
            {
                Id = r.Id,
                PurchaseInvoiceId = r.PurchaseInvoiceId,
                SupplierName = r.Supplier.Name,
                ReturnDate = r.ReturnDate,
                Reason = r.Reason,
                TotalAmount = r.TotalAmount
            }).ToList();
    }

    public async Task<PurchaseReturnViewModel?> GetByIdAsync(int id)
    {
        var item = await _purchaseReturnRepository.GetByIdWithDetailsAsync(id);

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
        var invoice = await _purchaseReturnRepository.GetInvoiceForCreateModelAsync(invoiceId);

        if (invoice == null) return null;

        var items = new List<PurchaseReturnItemViewModel>();
        foreach (var i in invoice.Items.Where(i => i.MedicineBatchId.HasValue))
        {
            var maxBoxes = await _purchaseReturnRepository.CountAvailableBatchBoxesAsync(i.MedicineId, i.BatchNumber);

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
        var invoice = await _purchaseReturnRepository.GetInvoiceWithItemsAsync(model.PurchaseInvoiceId);

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

        await using var transaction = await _transactionManager.BeginTransactionAsync();
        try
        {
            _unitOfWork.GetRepository<PurchaseReturn>().Add(purchaseReturn);
            await _unitOfWork.SaveChangesAsync();

            foreach (var line in returnItems)
            {
                var originalBatch = await _unitOfWork.GetRepository<MedicineBatch>().GetByIdAsync(line.MedicineBatchId);
                if (originalBatch == null) continue;

                var clonesToReturn = await _stockRepository.GetReturnableBatchClonesAsync(
                    line.MedicineId,
                    originalBatch.BatchNumber,
                    line.Quantity);

                if (clonesToReturn.Count < line.Quantity)
                    throw new InvalidOperationException($"Insufficient stock in batch {originalBatch.BatchNumber}.");

                foreach (var clone in clonesToReturn)
                {
                    await _stockService.DeductFromBatchAsync(clone.BatchId, clone.CurrentQuantity, purchaseReturn.Id);
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
