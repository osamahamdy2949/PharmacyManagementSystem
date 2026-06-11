namespace PharmacyManagement.DAL.Common;

public static class RoleNames
{
    public const string Administrator = "Administrator";
    public const string Pharmacist = "Pharmacist";
    public const string Cashier = "Cashier";

    public const string AdminOrPharmacist = $"{Administrator},{Pharmacist}";
    public const string AllStaff = $"{Administrator},{Pharmacist},{Cashier}";
}
