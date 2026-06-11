namespace PharmacyManagement.BLL.Services.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsInRole(string role);
}
