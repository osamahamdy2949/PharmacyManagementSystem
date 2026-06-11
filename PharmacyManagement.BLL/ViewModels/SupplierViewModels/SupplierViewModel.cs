using System.ComponentModel.DataAnnotations;
using PharmacyManagement.BLL.Common;

namespace PharmacyManagement.BLL.ViewModels.SupplierViewModels;

public class SupplierViewModel
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(11, MinimumLength = 11)]
    [RegularExpression(ValidationRules.EgyptianPhonePattern, ErrorMessage = ValidationRules.EgyptianPhoneMessage)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(50)]
    [Display(Name = "Tax Registration Number")]
    public string? TaxRegNumber { get; set; }
}
