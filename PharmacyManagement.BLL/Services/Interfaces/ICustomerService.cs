using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.CustomerViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerViewModel>> GetAllAsync();
    Task<CustomerViewModel?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(CustomerViewModel model);
    Task<ServiceResult> UpdateAsync(CustomerViewModel model, bool canManage);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<int>> GetOrCreateByNamePhoneAsync(string name, string? phone);
}
