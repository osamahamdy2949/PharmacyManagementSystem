using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.Validators;
using PharmacyManagement.BLL.ViewModels;
using PharmacyManagement.BLL.ViewModels.CategoryViewModels;
using PharmacyManagement.BLL.ViewModels.CustomerViewModels;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;
using PharmacyManagement.BLL.ViewModels.SupplierViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class MedicineService : IMedicineService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<MedicineViewModel> _createValidator;
    private readonly IValidator<MedicineViewModel> _editValidator;

    public MedicineService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<MedicineViewModel> createValidator,
        MedicineEditViewModelValidator editValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _editValidator = editValidator;
    }

    public async Task<IReadOnlyList<MedicineViewModel>> GetAllAsync(string? search = null)
    {
        IQueryable<Medicine> query = _unitOfWork.GetRepository<Medicine>().Query()
            .AsNoTracking()
            .Include(m => m.Category);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(m =>
                m.TradeName.ToLower().Contains(term) ||
                m.ScientificName.ToLower().Contains(term));
        }

        var items = await query.OrderBy(m => m.TradeName).ToListAsync();
        var vms = _mapper.Map<IReadOnlyList<MedicineViewModel>>(items);
        await PopulateStockInfoAsync(vms);
        return vms;
    }

    public async Task<SearchResultsViewModel> SearchAsync(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new SearchResultsViewModel { Query = query };
        }

        var term = query.Trim().ToLower();

        var medicines = await _unitOfWork.GetRepository<Medicine>().Query()
            .AsNoTracking()
            .Include(m => m.Category)
            .Where(m =>
                m.SerialNumber.ToLower().Contains(term) ||
                m.TradeName.ToLower().Contains(term) ||
                m.ScientificName.ToLower().Contains(term) ||
                m.Manufacturer.ToLower().Contains(term))
            .Take(20)
            .ToListAsync();

        var medicineVms = _mapper.Map<IReadOnlyList<MedicineViewModel>>(medicines);
        await PopulateStockInfoAsync(medicineVms);

        var categories = await _unitOfWork.GetRepository<Category>().Query()
            .AsNoTracking()
            .Where(c => c.Name.ToLower().Contains(term))
            .Take(10)
            .ToListAsync();

        var suppliers = await _unitOfWork.GetRepository<Supplier>().Query()
            .AsNoTracking()
            .Where(s => s.Name.ToLower().Contains(term) || (s.Email != null && s.Email.ToLower().Contains(term)))
            .Take(10)
            .ToListAsync();

        var customers = await _unitOfWork.GetRepository<Customer>().Query()
            .AsNoTracking()
            .Where(c => c.Name.ToLower().Contains(term) || (c.Phone != null && c.Phone.Contains(term)))
            .Take(10)
            .ToListAsync();

        return new SearchResultsViewModel
        {
            Query = query,
            Medicines = medicineVms,
            Categories = _mapper.Map<IReadOnlyList<CategoryViewModel>>(categories),
            Suppliers = _mapper.Map<IReadOnlyList<SupplierViewModel>>(suppliers),
            Customers = _mapper.Map<IReadOnlyList<CustomerViewModel>>(customers)
        };
    }

    public async Task<MedicineViewModel?> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.GetRepository<Medicine>().Query()
            .AsNoTracking()
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (item == null) return null;
        var vm = _mapper.Map<MedicineViewModel>(item);
        await PopulateStockInfoAsync(new[] { vm });
        return vm;
    }

    public async Task<ServiceResult> CreateAsync(MedicineViewModel model)
    {
        var validation = await ValidationHelper.ValidateAsync(_createValidator, model);
        if (validation != null) return validation;

        if (!await _unitOfWork.GetRepository<Category>().AnyAsync(c => c.Id == model.CategoryId))
            return ServiceResult.Fail("Category does not exist.");

        if (await _unitOfWork.GetRepository<Medicine>().AnyAsync(m => m.SerialNumber == model.SerialNumber))
            return ServiceResult.Fail("Serial number already exists.");

        var entity = _mapper.Map<Medicine>(model);
        entity.PurchasePrice = 0;
        entity.SellingPrice = 0;
        entity.UnitsPerPurchaseUnit = 1;
        _unitOfWork.GetRepository<Medicine>().Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(MedicineViewModel model)
    {
        var validation = await ValidationHelper.ValidateAsync(_editValidator, model);
        if (validation != null) return validation;

        var entity = await _unitOfWork.GetRepository<Medicine>().Query()
            .FirstOrDefaultAsync(m => m.Id == model.Id);
        if (entity == null) return ServiceResult.Fail("Medicine not found.");

        if (!await _unitOfWork.GetRepository<Category>().AnyAsync(c => c.Id == model.CategoryId))
            return ServiceResult.Fail("Category does not exist.");

        if (await _unitOfWork.GetRepository<Medicine>().Query()
            .AnyAsync(m => m.SerialNumber == model.SerialNumber && m.Id != model.Id))
            return ServiceResult.Fail("Serial number already exists.");

        entity.SerialNumber = model.SerialNumber;
        entity.TradeName = model.TradeName;
        entity.ScientificName = model.ScientificName;
        entity.Description = model.Description;
        entity.MedicineForm = model.MedicineForm;
        entity.Manufacturer = model.Manufacturer;
        entity.CategoryId = model.CategoryId;

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var hasPurchases = await _unitOfWork.GetRepository<PurchaseInvoiceItem>().AnyAsync(i => i.MedicineId == id);
        var hasSales = await _unitOfWork.GetRepository<SalesInvoiceItem>().AnyAsync(i => i.MedicineId == id);
        if (hasPurchases || hasSales)
            return ServiceResult.Fail("Cannot delete medicine linked to purchase or sales invoices.");

        var entity = await _unitOfWork.GetRepository<Medicine>().Query().FirstOrDefaultAsync(m => m.Id == id);
        if (entity == null) return ServiceResult.Fail("Medicine not found.");

        _unitOfWork.GetRepository<Medicine>().Remove(entity);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    private async Task PopulateStockInfoAsync(IEnumerable<MedicineViewModel> viewModels)
    {
        var medicines = viewModels.ToList();
        if (medicines.Count == 0)
            return;

        var today = DateTime.Today;
        var nearExpiry = today.AddDays(ValidationConstants.NearExpiryDays);
        var medicineIds = medicines.Select(m => m.Id).ToList();

        var stockInfo = await _unitOfWork.GetRepository<MedicineBatch>().Query()
            .AsNoTracking()
            .Where(b =>
                medicineIds.Contains(b.MedicineId) &&
                b.IsActive &&
                b.ExpiryDate > today &&
                b.CurrentQuantity > 0)
            .GroupBy(b => b.MedicineId)
            .Select(g => new
            {
                MedicineId = g.Key,
                QuantityInStock = g.Sum(b => b.CurrentQuantity),
                IsNearExpiry = g.Any(b => b.ExpiryDate <= nearExpiry)
            })
            .ToDictionaryAsync(x => x.MedicineId);

        foreach (var vm in medicines)
        {
            if (!stockInfo.TryGetValue(vm.Id, out var item))
                continue;

            vm.QuantityInStock = item.QuantityInStock;
            vm.IsNearExpiry = item.IsNearExpiry;
        }
    }
}
