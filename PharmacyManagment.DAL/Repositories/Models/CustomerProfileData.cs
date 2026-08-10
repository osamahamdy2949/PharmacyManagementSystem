using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Repositories.Models;

public class CustomerProfileData
{
    public Customer Customer { get; set; } = null!;
    public int TotalInvoices { get; set; }
}
