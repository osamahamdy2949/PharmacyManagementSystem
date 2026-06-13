using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.ShiftViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface IShiftService
{
    Task<ServiceResult> StartShiftAsync(StartShiftViewModel model);
    Task<ServiceResult> EndShiftAsync(EndShiftViewModel model);
    Task<ShiftViewModel?> GetActiveShiftAsync();
    Task<ShiftSummaryViewModel?> GetCurrentShiftSummaryAsync();
    Task<IReadOnlyList<ShiftViewModel>> GetShiftHistoryAsync(DateTime? from, DateTime? to);
    Task<ShiftViewModel?> GetByIdAsync(int id);
}
