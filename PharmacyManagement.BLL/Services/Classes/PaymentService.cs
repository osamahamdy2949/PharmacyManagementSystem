using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.Validators;
using PharmacyManagement.BLL.ViewModels.PaymentViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Data.Entities.Enums;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<RecordPaymentViewModel> _validator;
    private readonly ICurrentUserService _currentUser;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<RecordPaymentViewModel> validator,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult> RecordPaymentAsync(RecordPaymentViewModel model)
    {
        var validation = await ValidationHelper.ValidateAsync(_validator, model);
        if (validation != null) return validation;

        var activeShiftExists = await _unitOfWork.GetRepository<Shift>().Query()
            .AnyAsync(s => s.UserId == _currentUser.UserId && s.IsActive);
        if (!activeShiftExists)
            return ServiceResult.Fail("You must start a shift before recording payments.");

        await using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
        try
        {
            var customer = await _unitOfWork.GetRepository<Customer>().GetByIdAsync(model.CustomerId, tracking: true);
            if (customer == null)
                return ServiceResult.Fail("Customer not found.");

            SalesInvoice? invoice = null;
            if (model.SalesInvoiceId.HasValue)
            {
                invoice = await _unitOfWork.GetRepository<SalesInvoice>().GetByIdAsync(model.SalesInvoiceId.Value, tracking: true);
                if (invoice == null || invoice.CustomerId != model.CustomerId)
                    return ServiceResult.Fail("Invalid invoice for this customer.");

                if (invoice.RemainingAmount < model.AmountPaid)
                    return ServiceResult.Fail("Payment amount exceeds invoice remaining balance.");
            }
            else
            {
                if (customer.TotalDebt < model.AmountPaid)
                    return ServiceResult.Fail("Payment amount exceeds customer total debt.");
            }

            var payment = new Payment
            {
                CustomerId = model.CustomerId,
                SalesInvoiceId = model.SalesInvoiceId,
                AmountPaid = model.AmountPaid,
                PaymentDate = model.PaymentDate,
                PaymentMethod = model.PaymentMethod,
                Notes = model.Notes,
                RecordedByUserId = _currentUser.UserId
            };

            _unitOfWork.GetRepository<Payment>().Add(payment);

            // Update Customer
            customer.TotalPaid += model.AmountPaid;
            customer.TotalDebt -= model.AmountPaid;
            customer.RemainingBalance = customer.TotalDebt;
            customer.LastPaymentDate = model.PaymentDate;

            // Update Invoice if specified
            if (invoice != null)
            {
                invoice.PaidAmount += model.AmountPaid;
                invoice.RemainingAmount -= model.AmountPaid;
                invoice.PaymentStatus = invoice.RemainingAmount <= 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
            }
            // If no specific invoice is specified, we need to distribute the payment across oldest unpaid invoices
            else
            {
                var unpaidInvoices = await _unitOfWork.GetRepository<SalesInvoice>().Query()
                    .Where(s => s.CustomerId == model.CustomerId && s.RemainingAmount > 0)
                    .OrderBy(s => s.InvoiceDate)
                    .ToListAsync();

                var amountToDistribute = model.AmountPaid;
                foreach (var inv in unpaidInvoices)
                {
                    if (amountToDistribute <= 0) break;

                    var amountForThisInvoice = Math.Min(inv.RemainingAmount, amountToDistribute);
                    inv.PaidAmount += amountForThisInvoice;
                    inv.RemainingAmount -= amountForThisInvoice;
                    inv.PaymentStatus = inv.RemainingAmount <= 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;
                    
                    amountToDistribute -= amountForThisInvoice;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult.Fail($"Error recording payment: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<PaymentViewModel>> GetPaymentHistoryAsync(PaymentHistoryFilterViewModel filter)
    {
        var query = _unitOfWork.GetRepository<Payment>().Query()
            .Include(p => p.Customer)
            .AsQueryable();

        if (filter.CustomerId.HasValue)
            query = query.Where(p => p.CustomerId == filter.CustomerId.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(p => p.PaymentDate >= filter.FromDate.Value.Date);

        if (filter.ToDate.HasValue)
        {
            var endOfDay = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.PaymentDate <= endOfDay);
        }

        var payments = await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
        return await MapWithUserNamesAsync(payments);
    }

    public async Task<IReadOnlyList<PaymentViewModel>> GetPaymentsByCustomerAsync(int customerId)
    {
        var payments = await _unitOfWork.GetRepository<Payment>().Query()
            .Include(p => p.Customer)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
            
        return await MapWithUserNamesAsync(payments);
    }

    public async Task<IReadOnlyList<PaymentViewModel>> GetPaymentsByInvoiceAsync(int invoiceId)
    {
        var payments = await _unitOfWork.GetRepository<Payment>().Query()
            .Include(p => p.Customer)
            .Where(p => p.SalesInvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
            
        return await MapWithUserNamesAsync(payments);
    }

    public async Task<PaymentViewModel?> GetByIdAsync(int id)
    {
        var payment = await _unitOfWork.GetRepository<Payment>().Query()
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null) return null;

        var vm = _mapper.Map<PaymentViewModel>(payment);
        
        if (payment.RecordedByUserId != null)
        {
            var user = await _unitOfWork.Context.Set<ApplicationUser>().FirstOrDefaultAsync(u => u.Id == payment.RecordedByUserId);
            vm.RecordedByUserName = user?.FullName ?? "System";
        }
        
        return vm;
    }
    
    private async Task<IReadOnlyList<PaymentViewModel>> MapWithUserNamesAsync(List<Payment> payments)
    {
        var viewModels = _mapper.Map<List<PaymentViewModel>>(payments);

        var userIds = payments.Select(p => p.RecordedByUserId).Where(id => id != null).Distinct().ToList();
        if (userIds.Any())
        {
            var users = await _unitOfWork.Context.Set<ApplicationUser>()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            for (int i = 0; i < payments.Count; i++)
            {
                if (payments[i].RecordedByUserId != null && users.TryGetValue(payments[i].RecordedByUserId!, out var fullName))
                {
                    viewModels[i].RecordedByUserName = fullName;
                }
                else
                {
                    viewModels[i].RecordedByUserName = "System / Unknown";
                }
            }
        }

        return viewModels;
    }
}
