using AutoMapper;
using FluentValidation;
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
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISalesInvoiceRepository _salesInvoiceRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IDataTransactionManager _transactionManager;
    private readonly IMapper _mapper;
    private readonly IValidator<RecordPaymentViewModel> _validator;
    private readonly ICurrentUserService _currentUser;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IPaymentRepository paymentRepository,
        ISalesInvoiceRepository salesInvoiceRepository,
        IShiftRepository shiftRepository,
        IApplicationUserRepository applicationUserRepository,
        IDataTransactionManager transactionManager,
        IMapper mapper,
        IValidator<RecordPaymentViewModel> validator,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _paymentRepository = paymentRepository;
        _salesInvoiceRepository = salesInvoiceRepository;
        _shiftRepository = shiftRepository;
        _applicationUserRepository = applicationUserRepository;
        _transactionManager = transactionManager;
        _mapper = mapper;
        _validator = validator;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult> RecordPaymentAsync(RecordPaymentViewModel model)
    {
        var validation = await ValidationHelper.ValidateAsync(_validator, model);
        if (validation != null) return validation;

        var activeShiftExists = await _shiftRepository.HasActiveShiftAsync(_currentUser.UserId);
        if (!activeShiftExists)
            return ServiceResult.Fail("You must start a shift before recording payments.");

        await using var transaction = await _transactionManager.BeginTransactionAsync();
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
                var unpaidInvoices = await _salesInvoiceRepository.GetUnpaidInvoicesByCustomerAsync(model.CustomerId);

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
        var payments = await _paymentRepository.GetHistoryAsync(filter.CustomerId, filter.FromDate, filter.ToDate);
        return await MapWithUserNamesAsync(payments);
    }

    public async Task<IReadOnlyList<PaymentViewModel>> GetPaymentsByCustomerAsync(int customerId)
    {
        var payments = await _paymentRepository.GetByCustomerAsync(customerId);
            
        return await MapWithUserNamesAsync(payments);
    }

    public async Task<IReadOnlyList<PaymentViewModel>> GetPaymentsByInvoiceAsync(int invoiceId)
    {
        var payments = await _paymentRepository.GetByInvoiceAsync(invoiceId);
            
        return await MapWithUserNamesAsync(payments);
    }

    public async Task<PaymentViewModel?> GetByIdAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdWithCustomerAsync(id);

        if (payment == null) return null;

        var vm = _mapper.Map<PaymentViewModel>(payment);
        
        if (payment.RecordedByUserId != null)
        {
            vm.RecordedByUserName = await _applicationUserRepository.GetFullNameByIdAsync(payment.RecordedByUserId) ?? "System";
        }
        
        return vm;
    }
    
    private async Task<IReadOnlyList<PaymentViewModel>> MapWithUserNamesAsync(IReadOnlyList<Payment> payments)
    {
        var viewModels = _mapper.Map<List<PaymentViewModel>>(payments);

        var userIds = payments.Select(p => p.RecordedByUserId).Where(id => id != null).Select(id => id!).Distinct().ToList();
        if (userIds.Any())
        {
            var users = await _applicationUserRepository.GetFullNamesByIdsAsync(userIds);

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
