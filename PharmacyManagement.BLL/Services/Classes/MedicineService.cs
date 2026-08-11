using AutoMapper;
using FluentValidation;
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
    private readonly IMedicineRepository _medicineRepository;
    private readonly ISearchRepository _searchRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<MedicineViewModel> _validator;

    public MedicineService(
        IUnitOfWork unitOfWork,
        IMedicineRepository medicineRepository,
        ISearchRepository searchRepository,
        IMapper mapper,
        IValidator<MedicineViewModel> validator)
    {
        _unitOfWork = unitOfWork;
        _medicineRepository = medicineRepository;
        _searchRepository = searchRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<IReadOnlyList<MedicineViewModel>> GetAllAsync(string? search = null)
    {
        var items = await _medicineRepository.GetAllWithCategoryAsync(search);

        var viewModels = _mapper.Map<IReadOnlyList<MedicineViewModel>>(items);

        await PopulateStockInfoAsync(viewModels);

        return viewModels;
    }

    public async Task<SearchResultsViewModel> SearchAsync(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new SearchResultsViewModel
            {
                Query = query
            };
        }

        var term = query.Trim().ToLower();

        var medicines = await _medicineRepository.SearchWithCategoryAsync(term, 20);

        var medicineViewModels = _mapper.Map<IReadOnlyList<MedicineViewModel>>(medicines);

        await PopulateStockInfoAsync(medicineViewModels);

        var categories = await _searchRepository.SearchCategoriesAsync(term, 10);

        var suppliers = await _searchRepository.SearchSuppliersAsync(term, 10);

        var customers = await _searchRepository.SearchCustomersAsync(term, 10);

        return new SearchResultsViewModel
        {
            Query = query,
            Medicines = medicineViewModels,
            Categories = _mapper.Map<IReadOnlyList<CategoryViewModel>>(categories),
            Suppliers = _mapper.Map<IReadOnlyList<SupplierViewModel>>(suppliers),
            Customers = _mapper.Map<IReadOnlyList<CustomerViewModel>>(customers)
        };
    }

    public async Task<MedicineViewModel?> GetByIdAsync(int id)
    {
        var item = await _medicineRepository.GetByIdWithCategoryAsync(id);

        if (item == null)
            return null;

        var viewModel = _mapper.Map<MedicineViewModel>(item);

        await PopulateStockInfoAsync(new[] { viewModel });

        return viewModel;
    }

    public async Task<ServiceResult> CreateAsync(MedicineViewModel model)
    {
        var validation = await ValidationHelper.ValidateAsync(_validator, model);

        if (validation != null)
            return validation;

        var categoryExists = await _unitOfWork.GetRepository<Category>().AnyAsync(c => c.Id == model.CategoryId);

        if (!categoryExists)
            return ServiceResult.Fail("Category does not exist.");

        var serialNumberExists = await _unitOfWork.GetRepository<Medicine>().AnyAsync(m => m.SerialNumber == model.SerialNumber);

        if (serialNumberExists)
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
        var validation = await ValidationHelper.ValidateAsync(_validator, model);

        if (validation != null)
            return validation;

        var entity = await _medicineRepository.GetTrackedByIdAsync(model.Id);

        if (entity == null)
            return ServiceResult.Fail("Medicine not found.");

        var categoryExists = await _unitOfWork.GetRepository<Category>().AnyAsync(c => c.Id == model.CategoryId);

        if (!categoryExists)
            return ServiceResult.Fail("Category does not exist.");

        var serialNumberExists = await _medicineRepository.SerialNumberExistsForOtherMedicineAsync(model.SerialNumber,model.Id);

        if (serialNumberExists)
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
        {
            return ServiceResult.Fail(
                "Cannot delete medicine linked to purchase or sales invoices.");
        }

        var entity = await _medicineRepository.GetTrackedByIdAsync(id);

        if (entity == null)
            return ServiceResult.Fail("Medicine not found.");

        _unitOfWork.GetRepository<Medicine>().Delete(entity);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private async Task PopulateStockInfoAsync(
        IEnumerable<MedicineViewModel> viewModels)
    {
        var medicines = viewModels.ToList();

        if (medicines.Count == 0)
            return;

        var today = DateTime.Today;
        var nearExpiry =
            today.AddDays(ValidationConstants.NearExpiryDays);

        var medicineIds =
            medicines.Select(m => m.Id).ToList();

        var stockInfo =
            (await _medicineRepository.GetStockInfoAsync(
                medicineIds,
                today,
                nearExpiry))
            .ToDictionary(x => x.MedicineId);

        foreach (var viewModel in medicines)
        {
            if (!stockInfo.TryGetValue(viewModel.Id, out var item))
                continue;

            viewModel.QuantityInStock = item.QuantityInStock;
            viewModel.IsNearExpiry = item.IsNearExpiry;
        }
    }
}