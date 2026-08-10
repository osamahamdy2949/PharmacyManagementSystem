using AutoMapper;
using FluentValidation;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.Validators;
using PharmacyManagement.BLL.ViewModels.SupplierViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<SupplierViewModel> _validator;

    public SupplierService(
        IUnitOfWork unitOfWork,
        ISupplierRepository supplierRepository,
        IMapper mapper,
        IValidator<SupplierViewModel> validator)
    {
        _unitOfWork = unitOfWork;
        _supplierRepository = supplierRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<IReadOnlyList<SupplierViewModel>> GetAllAsync()
    {
        var items = await _unitOfWork.GetRepository<Supplier>().GetAllAsync();
        return _mapper.Map<IReadOnlyList<SupplierViewModel>>(items);
    }

    public async Task<SupplierViewModel?> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.GetRepository<Supplier>().GetByIdAsync(id);
        return item == null ? null : _mapper.Map<SupplierViewModel>(item);
    }

    public async Task<ServiceResult> CreateAsync(SupplierViewModel model)
    {
        var validation = await ValidationHelper.ValidateAsync(_validator, model);
        if (validation != null) return validation;

        if (await _unitOfWork.GetRepository<Supplier>().AnyAsync(s => s.Email == model.Email))
            return ServiceResult.Fail("Email already exists.");

        if (await _unitOfWork.GetRepository<Supplier>().AnyAsync(s => s.Phone == model.Phone))
            return ServiceResult.Fail("Phone number already exists.");

        var entity = _mapper.Map<Supplier>(model);
        _unitOfWork.GetRepository<Supplier>().Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(SupplierViewModel model)
    {
        var validation = await ValidationHelper.ValidateAsync(_validator, model);
        if (validation != null) return validation;

        var entity = await _unitOfWork.GetRepository<Supplier>().GetByIdAsync(model.Id);
        if (entity == null) return ServiceResult.Fail("Supplier not found.");

        var emailExists = await _supplierRepository.EmailExistsForOtherSupplierAsync(model.Email, model.Id);
        if (emailExists) return ServiceResult.Fail("Email already exists.");

        var phoneExists = await _supplierRepository.PhoneExistsForOtherSupplierAsync(model.Phone, model.Id);
        if (phoneExists) return ServiceResult.Fail("Phone number already exists.");

        entity.Email = model.Email;
        entity.Phone = model.Phone;
        entity.Address = model.Address;
        entity.TaxRegNumber = model.TaxRegNumber;
        _unitOfWork.GetRepository<Supplier>().Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.GetRepository<Supplier>().GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Supplier not found.");

        _unitOfWork.GetRepository<Supplier>().Delete(entity);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
