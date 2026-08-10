using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
{
    public SupplierRepository(PharmacyDbContext context) : base(context)
    {
    }

    public Task<bool> EmailExistsForOtherSupplierAsync(string email, int supplierId, CancellationToken ct = default) =>
        Query().AsNoTracking().AnyAsync(s => s.Email == email && s.Id != supplierId, ct);

    public Task<bool> PhoneExistsForOtherSupplierAsync(string phone, int supplierId, CancellationToken ct = default) =>
        Query().AsNoTracking().AnyAsync(s => s.Phone == phone && s.Id != supplierId, ct);
}
