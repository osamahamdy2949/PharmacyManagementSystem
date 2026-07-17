using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.ShiftViewModels;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.PL.Helpers;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AllStaff)]
public class ShiftController : Controller
{
    private readonly IShiftService _shiftService;

    public ShiftController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    public async Task<IActionResult> Start()
    {
        var activeShift = await _shiftService.GetActiveShiftAsync();
        if (activeShift != null)
        {
            TempData["Info"] = "You already have an active shift.";
            return RedirectToAction(nameof(Current));
        }

        return View(new StartShiftViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(StartShiftViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _shiftService.StartShiftAsync(model);
        if (!result.Success)
        {
            ModelState.AddServiceErrors(result);
            return View(model);
        }

        TempData["Success"] = "Shift started successfully.";
        return RedirectToAction("Pos", "SalesInvoice"); // Redirect to POS directly
    }

    public async Task<IActionResult> Current()
    {
        var activeShift = await _shiftService.GetActiveShiftAsync();
        if (activeShift == null)
            return RedirectToAction(nameof(Start));

        var summary = await _shiftService.GetCurrentShiftSummaryAsync();
        return View(summary);
    }

    public async Task<IActionResult> End(int? id)
    {
        var summary = id.HasValue
            ? await _shiftService.GetShiftSummaryAsync(id.Value)
            : await _shiftService.GetCurrentShiftSummaryAsync();

        if (summary == null)
        {
            if (id.HasValue)
                return Forbid();

            TempData["Error"] = "No active shift found.";
            return RedirectToAction("Index", "Home");
        }

        var model = new EndShiftViewModel
        {
            ShiftId = summary.ShiftId,
            ClosingCash = summary.ExpectedCash // Default to expected
        };

        ViewBag.Summary = summary;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> End(EndShiftViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Summary = await _shiftService.GetShiftSummaryAsync(model.ShiftId);
            return View(model);
        }

        var result = await _shiftService.EndShiftAsync(model);
        if (!result.Success)
        {
            ModelState.AddServiceErrors(result);
            ViewBag.Summary = await _shiftService.GetShiftSummaryAsync(model.ShiftId);
            return View(model);
        }

        TempData["Success"] = "Shift ended successfully.";
        return RedirectToAction("Details", new { id = model.ShiftId });
    }

    public async Task<IActionResult> History(DateTime? from, DateTime? to, int page = 1)
    {
        var shifts = await _shiftService.GetShiftHistoryAsync(from, to);
        ViewBag.From = from;
        ViewBag.To = to;
        return View(PagedList<ShiftViewModel>.Create(shifts, page));
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var shift = await _shiftService.GetByIdAsync(id);
        if (shift == null) return NotFound();
        return View(shift);
    }
}
