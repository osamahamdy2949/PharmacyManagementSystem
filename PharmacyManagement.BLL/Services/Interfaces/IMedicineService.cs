using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface IMedicineService
{
    Task<IReadOnlyList<MedicineViewModel>> GetAllAsync(string? search = null);
    Task<SearchResultsViewModel> SearchAsync(string? query);
    Task<MedicineViewModel?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(MedicineViewModel model);
    Task<ServiceResult> UpdateAsync(MedicineViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
}
