namespace PharmacyManagement.BLL.Common;

public static class ValidationRules
{
    /// <summary>Egyptian mobile: 010, 011, 012, or 015 + 8 digits (11 total).</summary>
    public const string EgyptianPhonePattern = @"^(010|011|012|015)\d{8}$";

    public const string EgyptianPhoneMessage =
        "Phone must be 11 digits and start with 010, 011, 012, or 015.";
}
