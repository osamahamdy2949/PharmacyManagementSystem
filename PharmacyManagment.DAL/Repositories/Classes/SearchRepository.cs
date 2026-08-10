using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class SearchRepository : ISearchRepository
{
    private readonly PharmacyDbContext _context;

    public SearchRepository(PharmacyDbContext context) => _context = context;

    public async Task<IReadOnlyList<Category>> SearchCategoriesAsync(string term, int take, CancellationToken ct = default) =>
        await _context.Set<Category>()
            .AsNoTracking()
            .Where(c => c.Name.ToLower().Contains(term))
            .Take(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Supplier>> SearchSuppliersAsync(string term, int take, CancellationToken ct = default) =>
        await _context.Set<Supplier>()
            .AsNoTracking()
            .Where(s => s.Name.ToLower().Contains(term) || (s.Email != null && s.Email.ToLower().Contains(term)))
            .Take(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Customer>> SearchCustomersAsync(string term, int take, CancellationToken ct = default) =>
        await _context.Set<Customer>()
            .AsNoTracking()
            .Where(c => c.Name.ToLower().Contains(term) || (c.Phone != null && c.Phone.Contains(term)))
            .Take(take)
            .ToListAsync(ct);
}
