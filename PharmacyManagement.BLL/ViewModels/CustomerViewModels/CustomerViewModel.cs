using System.ComponentModel.DataAnnotations;
using PharmacyManagement.BLL.Common;

namespace PharmacyManagement.BLL.ViewModels.CustomerViewModels;

public class CustomerViewModel
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(11)]
    [RegularExpression(ValidationRules.EgyptianPhonePattern, ErrorMessage = ValidationRules.EgyptianPhoneMessage)]
    public string? Phone { get; set; }

    public string PhoneDisplay => string.IsNullOrWhiteSpace(Phone) ? "No Phone" : Phone;

    public bool CanEditPhone { get; set; } = true;
    public bool CanEditName { get; set; }
}
