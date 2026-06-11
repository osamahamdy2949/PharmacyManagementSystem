using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DAL.Common;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AdminOrPharmacist)]
public class ReturnsController : Controller
{
    public IActionResult Index() => View();
}
