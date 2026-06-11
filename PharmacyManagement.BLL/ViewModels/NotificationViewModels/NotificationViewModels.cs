using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.BLL.ViewModels.NotificationViewModels;

public class NotificationViewModel
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
