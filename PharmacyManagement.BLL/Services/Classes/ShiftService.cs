using AutoMapper;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.ShiftViewModels;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Data.Entities.Enums;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class ShiftService : IShiftService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShiftRepository _shiftRepository;
    private readonly ISalesInvoiceRepository _salesInvoiceRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public ShiftService(
        IUnitOfWork unitOfWork,
        IShiftRepository shiftRepository,
        ISalesInvoiceRepository salesInvoiceRepository,
        IApplicationUserRepository applicationUserRepository,
        IMapper mapper,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _shiftRepository = shiftRepository;
        _salesInvoiceRepository = salesInvoiceRepository;
        _applicationUserRepository = applicationUserRepository;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult> StartShiftAsync(StartShiftViewModel model)
    {
        var existingShift = await GetActiveShiftEntityAsync();
        if (existingShift != null)
            return ServiceResult.Fail("You already have an active shift.");

        var shift = new Shift
        {
            UserId = _currentUser.UserId!,
            StartTime = DateTime.UtcNow,
            IsActive = true
        };

        _unitOfWork.GetRepository<Shift>().Add(shift);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> EndShiftAsync(EndShiftViewModel model)
    {
        var activeShift = await _shiftRepository.GetActiveShiftByIdAsync(model.ShiftId);

        if (activeShift == null)
            return ServiceResult.Fail("Invalid shift or shift is already closed.");

        var isOwnShift = activeShift.UserId == _currentUser.UserId;
        if (!isOwnShift && !_currentUser.IsInRole(RoleNames.Administrator))
            return ServiceResult.Fail("Only administrators can end another user's shift.");

        var summary = await CalculateShiftTotalsAsync(activeShift.UserId, activeShift.StartTime, DateTime.UtcNow);
        
        var expectedCash = summary.CashSales + summary.TotalPaymentsReceived - summary.TotalPurchases;
        
        activeShift.EndTime = DateTime.UtcNow;
        activeShift.IsActive = false;
        activeShift.ClosingCash = model.ClosingCash;
        activeShift.CashDifference = model.ClosingCash - expectedCash;
        activeShift.Notes = model.Notes;
        
        activeShift.TotalSales = summary.CashSales + summary.CreditSales;
        activeShift.CashSales = summary.CashSales;
        activeShift.CreditSales = summary.CreditSales;
        activeShift.TotalPurchases = summary.TotalPurchases;
        activeShift.TotalPaymentsReceived = summary.TotalPaymentsReceived;
        activeShift.NetCash = expectedCash;

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ShiftViewModel?> GetActiveShiftAsync()
    {
        var shift = await GetActiveShiftEntityAsync();
        if (shift == null) return null;

        var summary = await CalculateShiftTotalsAsync(shift.UserId, shift.StartTime, DateTime.UtcNow);
        var vm = _mapper.Map<ShiftViewModel>(shift);
        
        vm.UserName = _currentUser.UserName ?? "Unknown";
        vm.CashSales = summary.CashSales;
        vm.CreditSales = summary.CreditSales;
        vm.TotalSales = summary.CashSales + summary.CreditSales;
        vm.TotalPurchases = summary.TotalPurchases;
        vm.TotalPaymentsReceived = summary.TotalPaymentsReceived;
        
        return vm;
    }

    public async Task<ShiftSummaryViewModel?> GetCurrentShiftSummaryAsync()
    {
        var activeShift = await GetActiveShiftEntityAsync();
        if (activeShift == null) return null;

        return await BuildShiftSummaryAsync(activeShift);
    }

    public async Task<ShiftSummaryViewModel?> GetShiftSummaryAsync(int shiftId)
    {
        var shift = await _shiftRepository.GetActiveShiftByIdAsync(shiftId);

        if (shift == null)
            return null;

        if (shift.UserId != _currentUser.UserId && !_currentUser.IsInRole(RoleNames.Administrator))
            return null;

        return await BuildShiftSummaryAsync(shift);
    }

    private async Task<ShiftSummaryViewModel> BuildShiftSummaryAsync(Shift activeShift)
    {
        var userName = await _applicationUserRepository.GetFullNameByIdAsync(activeShift.UserId);
        var totals = await CalculateShiftTotalsAsync(activeShift.UserId, activeShift.StartTime, DateTime.UtcNow);

        return new ShiftSummaryViewModel
        {
            ShiftId = activeShift.Id,
            StartTime = activeShift.StartTime,
            UserName = userName ?? "Unknown",
            CashSales = totals.CashSales,
            TotalPaymentsReceived = totals.TotalPaymentsReceived,
            TotalPurchases = totals.TotalPurchases
        };
    }

    public async Task<IReadOnlyList<ShiftViewModel>> GetShiftHistoryAsync(DateTime? from, DateTime? to)
    {
        var shifts = await _shiftRepository.GetHistoryAsync(from, to);
        return await MapWithUserNamesAsync(shifts);
    }

    public async Task<ShiftViewModel?> GetByIdAsync(int id)
    {
        var shift = await _unitOfWork.GetRepository<Shift>().GetByIdAsync(id);
        if (shift == null) return null;

        var vm = _mapper.Map<ShiftViewModel>(shift);
        vm.UserName = await _applicationUserRepository.GetFullNameByIdAsync(shift.UserId) ?? "Unknown";

        return vm;
    }

    private async Task<Shift?> GetActiveShiftEntityAsync()
    {
        return await _shiftRepository.GetActiveShiftAsync(_currentUser.UserId);
    }

    private async Task<(decimal CashSales, decimal CreditSales, decimal TotalPurchases, decimal TotalPaymentsReceived)> CalculateShiftTotalsAsync(string userId, DateTime startTime, DateTime endTime)
    {
        var totals = await _salesInvoiceRepository.GetShiftTotalsAsync(userId, startTime, endTime);
        return (totals.CashSales, totals.CreditSales, totals.TotalPurchases, totals.TotalPaymentsReceived);
    }

    private async Task<IReadOnlyList<ShiftViewModel>> MapWithUserNamesAsync(IReadOnlyList<Shift> shifts)
    {
        var viewModels = _mapper.Map<List<ShiftViewModel>>(shifts);

        var userIds = shifts.Select(s => s.UserId).Distinct().ToList();
        if (userIds.Any())
        {
            var users = await _applicationUserRepository.GetFullNamesByIdsAsync(userIds);

            for (int i = 0; i < shifts.Count; i++)
            {
                if (users.TryGetValue(shifts[i].UserId, out var fullName))
                {
                    viewModels[i].UserName = fullName;
                }
            }
        }

        return viewModels;
    }
}
