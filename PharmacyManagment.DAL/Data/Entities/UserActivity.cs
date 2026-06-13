using Microsoft.AspNetCore.Identity;

namespace PharmacyManagement.DAL.Data.Entities;

public class UserActivity : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = default!;
    
    public string IPAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
}
