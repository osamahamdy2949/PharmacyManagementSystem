using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.Validators;
using PharmacyManagement.BLL.ViewModels.CustomerViewModels;
using PharmacyManagement.DAL.Data.Entities;
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

    public async Task<IReadOnlyList<CustomerViewModel>> GetAllAsync()
    {
        var items = await _unitOfWork.GetRepository<Customer>().GetAllAsync();
        return _mapper.Map<IReadOnlyList<CustomerViewModel>>(items);
    }

    public async Task<CustomerViewModel?> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.GetRepository<Customer>().GetByIdAsync(id);
        if (item == null) return null;
        var vm = _mapper.Map<CustomerViewModel>(item);
        return vm;
    }

    public async Task<ServiceResult> CreateAsync(CustomerViewModel model)
    {
        var validation = await ValidationHelper.ValidateAsync(_validator, model);
        if (validation != null) return validation;

        var entity = _mapper.Map<Customer>(model);
        _unitOfWork.GetRepository<Customer>().Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(CustomerViewModel model, bool canManage)
    {
        if (!canManage)
            return ServiceResult.Fail("You are not allowed to edit customers.");

        var entity = await _unitOfWork.GetRepository<Customer>().GetByIdAsync(model.Id);
        if (entity == null) return ServiceResult.Fail("Customer not found.");

        if (string.IsNullOrWhiteSpace(model.Phone))
            return ServiceResult.Fail("Phone number is required.");

        var validation = await ValidationHelper.ValidateAsync(_validator, model);
        if (validation != null) return validation;

        entity.Phone = model.Phone.Trim();

        _unitOfWork.GetRepository<Customer>().Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.GetRepository<Customer>().GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Customer not found.");

        _unitOfWork.GetRepository<Customer>().Remove(entity);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<int>> GetOrCreateByNamePhoneAsync(string name, string? phone)
    {
        var trimmedName = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            return ServiceResult<int>.Fail("Customer name is required.");

        var existing = await _unitOfWork.GetRepository<Customer>().Query()
            .FirstOrDefaultAsync(c =>
                c.Name.ToLower() == trimmedName.ToLower() &&
                (string.IsNullOrWhiteSpace(phone) || c.Phone == phone));

        if (existing != null)
            return ServiceResult<int>.Ok(existing.Id);

        var customer = new Customer
        {
            Name = trimmedName,
            Phone = phone?.Trim() ?? string.Empty
        };

        _unitOfWork.GetRepository<Customer>().Add(customer);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult<int>.Ok(customer.Id);
    }

    public async Task<CustomerProfileViewModel?> GetCustomerProfileAsync(int id)
    {
        var item = await _unitOfWork.GetRepository<Customer>().Query()
            .Include(c => c.SalesInvoices)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (item == null) return null;
        
        var vm = _mapper.Map<CustomerProfileViewModel>(item);
        vm.TotalInvoices = item.SalesInvoices.Count;
        return vm;
    }

    public async Task<IReadOnlyList<CustomerDebtHistoryViewModel>> GetCustomerDebtHistoryAsync(int id)
    {
        var invoices = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Where(s => s.CustomerId == id && s.SaleType == PharmacyManagement.DAL.Data.Entities.Enums.SaleType.Credit)
            .OrderByDescending(s => s.InvoiceDate)
            .ToListAsync();
            
        return invoices.Select(s => new CustomerDebtHistoryViewModel
        {
            InvoiceId = s.Id,
            InvoiceDate = s.InvoiceDate,
            TotalAmount = s.TotalAmount,
            PaidAmount = s.PaidAmount,
            RemainingAmount = s.RemainingAmount,
            PaymentStatus = s.PaymentStatus.ToString()
        }).ToList();
    }
}
