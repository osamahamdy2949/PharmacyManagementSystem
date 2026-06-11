using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.PurchaseInvoiceViewModels;
using PharmacyManagement.BLL.ViewModels.RestockViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface IPurchaseService
{
    Task<IReadOnlyList<PurchaseInvoiceViewModel>> GetAllAsync();
    Task<PurchaseInvoiceViewModel?> GetByIdAsync(int id);
    Task<ServiceResult<int>> CreateAsync(CreatePurchaseInvoiceViewModel model);
    Task<ServiceResult<int>> RestockFinishedMedicineAsync(RestockFinishedMedicineRequest request);
    Task<string> GetNextBatchNumberAsync();
    Task<IReadOnlyList<string>> GetExistingBatchNumbersAsync();
    Task<IReadOnlyList<PendingBatchViewModel>> GetPendingBatchesAsync();
    Task<ActivateBatchViewModel?> GetActivateBatchModelAsync(int batchId);
    Task<ServiceResult> ActivateBatchAsync(ActivateBatchViewModel model);
    Task<ServiceResult> EditPendingBatchQuantityAsync(int batchId, int newQuantity);
    Task<ServiceResult> DeletePendingBatchAsync(int batchId);
}
