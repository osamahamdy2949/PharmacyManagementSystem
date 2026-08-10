using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface ISearchRepository
{
    Task<IReadOnlyList<Category>> SearchCategoriesAsync(string term, int take, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> SearchSuppliersAsync(string term, int take, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> SearchCustomersAsync(string term, int take, CancellationToken ct = default);
}
