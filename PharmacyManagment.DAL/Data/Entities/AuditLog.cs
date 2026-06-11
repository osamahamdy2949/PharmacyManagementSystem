namespace PharmacyManagement.DAL.Data.Entities;

public class AuditLog : BaseEntity
{
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
