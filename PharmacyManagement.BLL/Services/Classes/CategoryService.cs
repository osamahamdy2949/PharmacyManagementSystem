using AutoMapper;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.CategoryViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoryViewModel>> GetAllAsync()
    {
        var items = await _unitOfWork.GetRepository<Category>().GetAllAsync();
        return _mapper.Map<IReadOnlyList<CategoryViewModel>>(items);
    }

    public async Task<CategoryViewModel?> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.GetRepository<Category>().GetByIdAsync(id);
        return item == null ? null : _mapper.Map<CategoryViewModel>(item);
    }

    public async Task<ServiceResult> CreateAsync(CategoryViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            return ServiceResult.Fail("Category name is required.");

        var entity = _mapper.Map<Category>(model);
        _unitOfWork.GetRepository<Category>().Add(entity);

        var result = await _unitOfWork.SaveChangesAsync();

        return result > 0 ? ServiceResult.Ok() : ServiceResult.Fail("Failed to create category.");
    }

    public async Task<ServiceResult> UpdateAsync(CategoryViewModel model)
    {
        var entity = await _unitOfWork.GetRepository<Category>().GetByIdAsync(model.Id);
        if (entity == null) return ServiceResult.Fail("Category not found.");

        _mapper.Map(model, entity);
        _unitOfWork.GetRepository<Category>().Update(entity);
        var result = await _unitOfWork.SaveChangesAsync();
        return result > 0 ? ServiceResult.Ok() : ServiceResult.Fail("Failed to update category.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.GetRepository<Category>().GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Category not found.");

        if (await _unitOfWork.GetRepository<Medicine>().AnyAsync(m => m.CategoryId == id))
            return ServiceResult.Fail("Cannot delete category that has medicines.");

        _unitOfWork.GetRepository<Category>().Remove(entity);
        var result = await _unitOfWork.SaveChangesAsync();
        return result > 0 ? ServiceResult.Ok() : ServiceResult.Fail("Failed to delete category.");
    }
}
