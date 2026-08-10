using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.NotificationViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Data.Entities.Enums;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(IUnitOfWork unitOfWork, INotificationRepository notificationRepository)
    {
        _unitOfWork = unitOfWork;
        _notificationRepository = notificationRepository;
    }

    public async Task<int> GetUnreadCountAsync(string? userId = null) =>
        await _notificationRepository.GetUnreadCountAsync(userId);

    public async Task<IReadOnlyList<NotificationViewModel>> GetRecentAsync(int count = 20) =>
        (await _notificationRepository.GetRecentAsync(count))
            .Select(n => new NotificationViewModel
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();

    public async Task GenerateStockAlertsAsync()
    {
        var today = DateTime.Today;
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);

        var lowStock = await _notificationRepository.GetLowStockMedicineNamesAsync(today);

        foreach (var name in lowStock)
            await AddIfNotExistsAsync(NotificationType.LowStock, $"Low stock alert: {name}");

        var nearExpiryMeds = await _notificationRepository.GetNearExpiryMedicineNamesAsync(today, nearExpiry);

        foreach (var name in nearExpiryMeds)
            await AddIfNotExistsAsync(NotificationType.NearExpiry, $"Near expiry alert: {name}");
    }

    public async Task MarkAsReadAsync(int id)
    {
        var n = await _unitOfWork.GetRepository<Notification>().GetByIdAsync(id, tracking: true);
        if (n == null) return;
        n.IsRead = true;
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task AddIfNotExistsAsync(NotificationType type, string message)
    {
        var exists = await _unitOfWork.GetRepository<Notification>().AnyAsync(n =>
            n.Type == type && n.Message == message && !n.IsRead && n.CreatedAt >= DateTime.UtcNow.AddDays(-1));

        if (exists) return;

        _unitOfWork.GetRepository<Notification>().Add(new Notification
        {
            Type = type,
            Message = message,
            IsRead = false
        });
        await _unitOfWork.SaveChangesAsync();
    }
}
