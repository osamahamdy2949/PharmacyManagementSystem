using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.BLL.Services.Interfaces;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AllStaff)]
public class SearchController : Controller
{
    private readonly IMedicineService _medicineService;

    public SearchController(IMedicineService medicineService) => _medicineService = medicineService;

    [HttpGet]
    public async Task<IActionResult> Index(string? q) =>
        View(await _medicineService.SearchAsync(q));
}
