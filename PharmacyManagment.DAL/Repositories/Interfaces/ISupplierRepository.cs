using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface ISupplierRepository : IGenericRepository<Supplier>
{
    Task<bool> EmailExistsForOtherSupplierAsync(string email, int supplierId, CancellationToken ct = default);
    Task<bool> PhoneExistsForOtherSupplierAsync(string phone, int supplierId, CancellationToken ct = default);
}
