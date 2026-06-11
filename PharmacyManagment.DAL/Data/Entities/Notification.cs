using PharmacyManagement.DAL.Data.Entities.Enums;

namespace PharmacyManagement.DAL.Data.Entities;

public class Notification : BaseEntity
{
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? UserId { get; set; }
    public int? ReferenceId { get; set; }
}
