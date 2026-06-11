using PharmacyManagement.BLL.ViewModels.CategoryViewModels;
using PharmacyManagement.BLL.ViewModels.CustomerViewModels;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;
using PharmacyManagement.BLL.ViewModels.SupplierViewModels;

namespace PharmacyManagement.BLL.ViewModels;

public class SearchResultsViewModel
{
    public string? Query { get; set; }
    public IReadOnlyList<MedicineViewModel> Medicines { get; set; } = Array.Empty<MedicineViewModel>();
    public IReadOnlyList<CategoryViewModel> Categories { get; set; } = Array.Empty<CategoryViewModel>();
    public IReadOnlyList<SupplierViewModel> Suppliers { get; set; } = Array.Empty<SupplierViewModel>();
    public IReadOnlyList<CustomerViewModel> Customers { get; set; } = Array.Empty<CustomerViewModel>();
}
