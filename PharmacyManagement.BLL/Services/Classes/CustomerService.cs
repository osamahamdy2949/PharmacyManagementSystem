using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.Validators;
using PharmacyManagement.BLL.ViewModels.CustomerViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Data.Entities.Enums;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CustomerViewModel> _validator;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CustomerViewModel> validator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<IReadOnlyList<CustomerViewModel>> GetAllAsync(CancellationToken ct)
    {
        var items = await _unitOfWork.GetRepository<Customer>().GetAllAsync(ct: ct);
        return _mapper.Map<IReadOnlyList<CustomerViewModel>>(items);
    }

    public async Task<CustomerViewModel?> GetByIdAsync(int id, CancellationToken ct)
    {
        var item = await _unitOfWork.GetRepository<Customer>().GetByIdAsync(id, ct: ct);
        return item == null ? null : _mapper.Map<CustomerViewModel>(item);
    }

    public async Task<ServiceResult> CreateAsync(CustomerViewModel model, CancellationToken ct)
    {
        var validation = await ValidationHelper.ValidateAsync(_validator, model);
        if (validation != null) return validation;

        var entity = _mapper.Map<Customer>(model);
        _unitOfWork.GetRepository<Customer>().Add(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(CustomerViewModel model, bool canManage, CancellationToken ct)
    {
        if (!canManage)
            return ServiceResult.Fail("You are not allowed to edit customers.");

        var entity = await _unitOfWork.GetRepository<Customer>().GetByIdAsync(model.Id, true, ct);
        if (entity == null) return ServiceResult.Fail("Customer not found.");

        if (string.IsNullOrWhiteSpace(model.Phone))
            return ServiceResult.Fail("Phone number is required.");

        var validation = await ValidationHelper.ValidateAsync(_validator, model);
        if (validation != null) return validation;

        entity.Phone = model.Phone.Trim();

        _unitOfWork.GetRepository<Customer>().Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _unitOfWork.GetRepository<Customer>().GetByIdAsync(id, ct:ct);
        if (entity == null) return ServiceResult.Fail("Customer not found.");

        _unitOfWork.GetRepository<Customer>().Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<int>> GetOrCreateByNamePhoneAsync(string name, string? phone, CancellationToken ct)
    {
        var trimmedName = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            return ServiceResult<int>.Fail("Customer name is required.");

        var normalizedName = trimmedName.ToUpper();
        
        var normalizedPhone = phone?.Trim();
        
        var existing = await _unitOfWork.GetRepository<Customer>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Name.ToUpper() == normalizedName &&
                (string.IsNullOrWhiteSpace(normalizedPhone) || c.Phone == normalizedPhone), ct);

        if (existing != null)
            return ServiceResult<int>.Ok(existing.Id);

        var customer = new Customer
        {
            Name = trimmedName,
            Phone = normalizedPhone ?? string.Empty
        };

        _unitOfWork.GetRepository<Customer>().Add(customer);
        await _unitOfWork.SaveChangesAsync(ct);
        return ServiceResult<int>.Ok(customer.Id);
    }

    public async Task<CustomerProfileViewModel?> GetCustomerProfileAsync(int id, CancellationToken ct)
    {
        var item = await _unitOfWork.GetRepository<Customer>().Query()
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                Customer = c,
                TotalInvoices = c.SalesInvoices.Count
            })
            .FirstOrDefaultAsync(ct);

        if (item == null)
            return null;

        var vm = _mapper.Map<CustomerProfileViewModel>(item.Customer);
        vm.TotalInvoices = item.TotalInvoices;
        return vm;
    }

    public async Task<IReadOnlyList<CustomerDebtHistoryViewModel>> GetCustomerDebtHistoryAsync(int id, CancellationToken ct)
    {
        var invoices = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .AsNoTracking()
            .Where(s => s.CustomerId == id && s.SaleType == SaleType.Credit)
            .OrderByDescending(s => s.InvoiceDate)
            .Select(s => new CustomerDebtHistoryViewModel
            {
                InvoiceId = s.Id,
                InvoiceDate = s.InvoiceDate,
                TotalAmount = s.TotalAmount,
                PaidAmount = s.PaidAmount,
                RemainingAmount = s.RemainingAmount,
                PaymentStatus = s.PaymentStatus.ToString()
            })
            .ToListAsync(ct);

        return invoices;
    }
}
