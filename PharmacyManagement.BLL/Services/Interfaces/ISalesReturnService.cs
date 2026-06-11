using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.SalesReturnViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface ISalesReturnService
{
    Task<IReadOnlyList<SalesReturnListItemViewModel>> GetAllAsync();
    Task<SalesReturnViewModel?> GetByIdAsync(int id);
    Task<CreateSalesReturnViewModel?> GetCreateModelFromInvoiceAsync(int invoiceId);
    Task<ServiceResult<int>> CreateAsync(CreateSalesReturnViewModel model);
    Task<int> GetReturnCountAsync();
}
