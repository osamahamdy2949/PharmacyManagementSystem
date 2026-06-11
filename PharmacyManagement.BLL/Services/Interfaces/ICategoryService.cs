using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.CategoryViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryViewModel>> GetAllAsync();
    Task<CategoryViewModel?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(CategoryViewModel model);
    Task<ServiceResult> UpdateAsync(CategoryViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
}
