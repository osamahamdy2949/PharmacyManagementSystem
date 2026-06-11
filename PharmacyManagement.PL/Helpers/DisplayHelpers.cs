using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;

namespace PharmacyManagement.PL.Helpers;

public static class DisplayHelpers
{
    public static string Egp(decimal amount) => CurrencyHelper.FormatEgp(amount);

    public static string Date(DateTime date) => DateDisplayHelper.FormatDate(date);

    public static string MedicineRowClass(MedicineViewModel medicine)
    {
        if (medicine.IsNearExpiry)
            return "row-near-expiry";
        if (medicine.IsLowStock)
            return "row-low-stock";
        return string.Empty;
    }
}
