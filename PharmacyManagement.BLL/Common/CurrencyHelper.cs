using System.Globalization;

namespace PharmacyManagement.BLL.Common;

public static class CurrencyHelper
{
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-GB");

    public static string FormatEgp(decimal amount) =>
        $"{amount.ToString("N2", DisplayCulture)} L.E";

    public static CultureInfo DisplayCultureInfo => DisplayCulture;
}
