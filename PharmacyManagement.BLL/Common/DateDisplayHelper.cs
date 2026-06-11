using System.Globalization;

namespace PharmacyManagement.BLL.Common;

public static class DateDisplayHelper
{
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-GB");

    public static string FormatDate(DateTime date) =>
        date.ToString("dd/MM/yyyy", DisplayCulture);

    public static string FormatDateTime(DateTime date) =>
        date.ToString("dd/MM/yyyy HH:mm", DisplayCulture);
}
