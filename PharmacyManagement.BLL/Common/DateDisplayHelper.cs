using System.Globalization;

namespace PharmacyManagement.BLL.Common;

public static class DateDisplayHelper
{
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-GB");

    public static string FormatDate(DateTime date) =>
        date.ToString("dd/MM/yyyy", DisplayCulture);

    public static string FormatDateTime(DateTime date) =>
        ToLocalDisplayTime(date).ToString("dd/MM/yyyy HH:mm", DisplayCulture);

    public static string FormatShortDateTime(DateTime date) =>
        ToLocalDisplayTime(date).ToString("g", DisplayCulture);

    private static DateTime ToLocalDisplayTime(DateTime date)
    {
        if (date.Kind == DateTimeKind.Local)
            return date;

        var utcDate = date.Kind == DateTimeKind.Utc
            ? date
            : DateTime.SpecifyKind(date, DateTimeKind.Utc);

        return utcDate.ToLocalTime();
    }
}
