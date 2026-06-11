using FluentValidation;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.SupplierViewModels;

namespace PharmacyManagement.BLL.Validators;

public class SupplierViewModelValidator : AbstractValidator<SupplierViewModel>
{
    public SupplierViewModelValidator()
    {
        RuleFor(s => s.Name).NotEmpty().MaximumLength(50);
        RuleFor(s => s.Email).NotEmpty().EmailAddress();
        RuleFor(s => s.Phone)
            .NotEmpty()
            .Length(11)
            .Matches(ValidationRules.EgyptianPhonePattern)
            .WithMessage(ValidationRules.EgyptianPhoneMessage);
    }
}
