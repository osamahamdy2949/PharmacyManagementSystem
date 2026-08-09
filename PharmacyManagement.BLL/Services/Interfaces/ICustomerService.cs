using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.CustomerViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerViewModel>> GetAllAsync(CancellationToken ct= default);
    Task<CustomerViewModel?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult> CreateAsync(CustomerViewModel model, CancellationToken ct = default);
    Task<ServiceResult> UpdateAsync(CustomerViewModel model, bool canManage, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> GetOrCreateByNamePhoneAsync(string name, string? phone, CancellationToken ct = default);
    Task<CustomerProfileViewModel?> GetCustomerProfileAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerDebtHistoryViewModel>> GetCustomerDebtHistoryAsync(int id, CancellationToken ct = default);
}
