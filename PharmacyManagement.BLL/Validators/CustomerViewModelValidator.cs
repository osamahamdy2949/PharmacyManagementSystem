using FluentValidation;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.CustomerViewModels;

namespace PharmacyManagement.BLL.Validators;

public class CustomerViewModelValidator : AbstractValidator<CustomerViewModel>
{
    public CustomerViewModelValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Phone)
            .NotEmpty()
            .Length(11)
            .Matches(ValidationRules.EgyptianPhonePattern)
            .WithMessage(ValidationRules.EgyptianPhoneMessage);
    }
}
