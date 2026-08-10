using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Interfaces;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<CustomerProfileData?> GetCustomerProfileAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerDebtHistoryData>> GetCustomerDebtHistoryAsync(int id, CancellationToken ct = default);
}
