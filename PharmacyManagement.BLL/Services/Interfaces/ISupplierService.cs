using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.SupplierViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface ISupplierService
{
    Task<IReadOnlyList<SupplierViewModel>> GetAllAsync();
    Task<SupplierViewModel?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(SupplierViewModel model);
    Task<ServiceResult> UpdateAsync(SupplierViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
}
