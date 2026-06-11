using PharmacyManagement.BLL.ViewModels.NotificationViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface INotificationService
{
    Task<int> GetUnreadCountAsync(string? userId = null);
    Task<IReadOnlyList<NotificationViewModel>> GetRecentAsync(int count = 20);
    Task GenerateStockAlertsAsync();
    Task MarkAsReadAsync(int id);
}
