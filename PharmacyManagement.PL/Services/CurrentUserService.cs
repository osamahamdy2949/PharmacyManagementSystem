using System.Security.Claims;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.DAL.Common;

namespace PharmacyManagement.PL.Services;

public class CurrentUserService : ICurrentUserService, IAuditUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
}
