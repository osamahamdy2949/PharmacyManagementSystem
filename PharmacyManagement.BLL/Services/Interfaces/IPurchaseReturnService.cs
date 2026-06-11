using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.PurchaseReturnViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface IPurchaseReturnService
{
    Task<IReadOnlyList<PurchaseReturnListItemViewModel>> GetAllAsync();
    Task<PurchaseReturnViewModel?> GetByIdAsync(int id);
    Task<CreatePurchaseReturnViewModel?> GetCreateModelFromInvoiceAsync(int invoiceId);
    Task<ServiceResult<int>> CreateAsync(CreatePurchaseReturnViewModel model);
}
