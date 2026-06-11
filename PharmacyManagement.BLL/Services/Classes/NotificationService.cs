using Microsoft.EntityFrameworkCore;
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

    public NotificationService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<int> GetUnreadCountAsync(string? userId = null) =>
        await _unitOfWork.GetRepository<Notification>().Query()
            .CountAsync(n => !n.IsRead && (userId == null || n.UserId == null || n.UserId == userId));

    public async Task<IReadOnlyList<NotificationViewModel>> GetRecentAsync(int count = 20) =>
        await _unitOfWork.GetRepository<Notification>().Query()
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .Select(n => new NotificationViewModel
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToListAsync();

    public async Task GenerateStockAlertsAsync()
    {
        var today = DateTime.Today;
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);

        var lowStock = await _unitOfWork.GetRepository<Medicine>().Query()
            .Where(m => m.MedicineBatches.Sum(b => (b.ExpiryDate > today && b.CurrentQuantity > 0) ? b.CurrentQuantity : 0) < m.MinStockLevel)
            .Select(m => m.TradeName)
            .ToListAsync();

        foreach (var name in lowStock)
            await AddIfNotExistsAsync(NotificationType.LowStock, $"Low stock alert: {name}");

        var nearExpiryMeds = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .Include(b => b.Medicine)
            .Where(b => b.ExpiryDate >= today && b.ExpiryDate <= nearExpiry && b.CurrentQuantity > 0)
            .Select(b => b.Medicine.TradeName)
            .Distinct()
            .ToListAsync();

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
