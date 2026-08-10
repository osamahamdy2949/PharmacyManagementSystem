using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Data.Entities.Enums;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class StockService : IStockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStockRepository _stockRepository;
    private readonly IDataTransactionManager _transactionManager;
    private readonly ICurrentUserService _currentUser;

    public StockService(IUnitOfWork unitOfWork, IStockRepository stockRepository, IDataTransactionManager transactionManager, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _stockRepository = stockRepository;
        _transactionManager = transactionManager;
        _currentUser = currentUser;
    }

    public async Task<int> GetAvailableStockAsync(int medicineId, CancellationToken ct = default)
    {
        var today = DateTime.Today;
        return await _stockRepository.GetAvailableStockAsync(medicineId, today, ct);
    }

    public async Task<int> GetAvailableStockAsync(int medicineId, string dose, CancellationToken ct = default)
    {
        var today = DateTime.Today;
        return await _stockRepository.GetAvailableStockAsync(medicineId, dose, today, ct);
    }

    public async Task<IReadOnlyList<MedicineBatch>> GetBatchesForSaleAsync(int medicineId, CancellationToken ct = default)
    {
        var today = DateTime.Today;
        return await _stockRepository.GetBatchesForSaleAsync(medicineId, today, ct);
    }

    public async Task<IReadOnlyList<MedicineBatch>> GetBatchesForSaleAsync(int medicineId, string dose, CancellationToken ct = default)
    {
        var today = DateTime.Today;
        return await _stockRepository.GetBatchesForSaleAsync(medicineId, dose, today, ct);
    }

    public async Task<IReadOnlyList<BatchDeduction>> DeductStockFefoAsync(
        int medicineId, int quantity, int? referenceId = null, int? preferredBatchId = null, string? dose = null, CancellationToken ct = default)
    {
        if (preferredBatchId.HasValue)
            return await DeductStockFromBatchAsync(preferredBatchId.Value, quantity, referenceId, ct);

        var batches = string.IsNullOrWhiteSpace(dose)
            ? await GetBatchesForSaleAsync(medicineId, ct)
            : await GetBatchesForSaleAsync(medicineId, dose, ct);
        return await DeductFromBatchesAsync(batches, medicineId, quantity, referenceId, ct);
    }

    public async Task<IReadOnlyList<BatchDeduction>> DeductStockFromBatchAsync(
        int batchId, int quantity, int? referenceId = null, CancellationToken ct = default)
    {
        var batch = await _unitOfWork.GetRepository<MedicineBatch>().GetByIdAsync(batchId, tracking: true, ct: ct)
            ?? throw new InvalidOperationException($"Batch {batchId} not found.");

        if (!batch.IsActive)
            throw new InvalidOperationException("Cannot sell from inactive stock.");

        return await DeductFromBatchesAsync(new[] { batch }, batch.MedicineId, quantity, referenceId, ct);
    }

    private async Task<IReadOnlyList<BatchDeduction>> DeductFromBatchesAsync(
        IEnumerable<MedicineBatch> batches, int medicineId, int quantity, int? referenceId, CancellationToken ct)
    {
        var remaining = quantity;
        var deductions = new List<BatchDeduction>();

        foreach (var batch in batches)
        {
            if (remaining <= 0) break;

            var deduct = Math.Min(batch.CurrentQuantity, remaining);
            batch.CurrentQuantity -= deduct;
            remaining -= deduct;
            deductions.Add(new BatchDeduction(batch.Id, deduct, batch.SellingPrice));
            await RecordTransactionAsync(medicineId, -deduct, StockTransactionType.Sale, batch.Id, referenceId, ct: ct);
        }

        if (remaining > 0)
            throw new InvalidOperationException($"Insufficient stock for medicine {medicineId}. Short by {remaining} units.");

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (_transactionManager.IsConcurrencyException(ex))
        {
            throw new InvalidOperationException("Stock was updated by another user. Please retry.");
        }

        return deductions;
    }

    public async Task<MedicineBatch> AddBatchStockAsync(
        MedicineBatch batch, int referenceId, bool recordTransaction = true, CancellationToken ct = default)
    {
        _unitOfWork.GetRepository<MedicineBatch>().Add(batch);
        await _unitOfWork.SaveChangesAsync(ct);
        if (recordTransaction && batch.IsActive)
            await RecordTransactionAsync(batch.MedicineId, batch.CurrentQuantity, StockTransactionType.Purchase, batch.Id, referenceId, ct: ct);
        return batch;
    }

    public async Task RestoreToBatchAsync(int batchId, int quantity, int referenceId, CancellationToken ct = default)
    {
        var batch = await _unitOfWork.GetRepository<MedicineBatch>().GetByIdAsync(batchId, tracking: true, ct: ct)
            ?? throw new InvalidOperationException($"Batch {batchId} not found.");

        if (!batch.IsActive)
            throw new InvalidOperationException("Cannot restore to inactive batch.");

        batch.CurrentQuantity += quantity;
        await RecordTransactionAsync(batch.MedicineId, quantity, StockTransactionType.SaleReturn, batch.Id, referenceId, ct: ct);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (_transactionManager.IsConcurrencyException(ex))
        {
            throw new InvalidOperationException("Stock was updated by another user. Please retry.");
        }
    }

    public async Task DeductFromBatchAsync(int batchId, int quantity, int referenceId, CancellationToken ct = default)
    {
        var batch = await _unitOfWork.GetRepository<MedicineBatch>().GetByIdAsync(batchId, tracking: true, ct: ct)
            ?? throw new InvalidOperationException($"Batch {batchId} not found.");

        if (!batch.IsActive)
            throw new InvalidOperationException("Cannot deduct from inactive batch.");

        if (batch.CurrentQuantity < quantity)
            throw new InvalidOperationException($"Insufficient stock in batch {batch.BatchNumber}.");

        batch.CurrentQuantity -= quantity;
        await RecordTransactionAsync(batch.MedicineId, -quantity, StockTransactionType.PurchaseReturn, batch.Id, referenceId, ct: ct);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (_transactionManager.IsConcurrencyException(ex))
        {
            throw new InvalidOperationException("Stock was updated by another user. Please retry.");
        }
    }

    public async Task RecordTransactionAsync(
        int medicineId, int quantity, StockTransactionType type, int? batchId, int? referenceId,
        string? notes = null, CancellationToken ct = default)
    {
        _unitOfWork.GetRepository<StockTransaction>().Add(new StockTransaction
        {
            MedicineId = medicineId,
            MedicineBatchId = batchId,
            TransactionType = type,
            Quantity = quantity,
            ReferenceId = referenceId,
            UserId = _currentUser.UserId,
            Notes = notes
        });
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
