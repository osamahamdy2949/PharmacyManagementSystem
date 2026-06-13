using PharmacyManagement.BLL.ViewModels.DashboardViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
}
