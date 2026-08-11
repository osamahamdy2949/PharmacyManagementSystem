using FluentValidation;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;

namespace PharmacyManagement.BLL.Validators;

public class MedicineViewModelValidator : AbstractValidator<MedicineViewModel>
{
    public MedicineViewModelValidator()
    {
        RuleFor(m => m.SerialNumber)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(m => m.MedicineForm)
            .IsInEnum();

        RuleFor(m => m.TradeName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(m => m.ScientificName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(m => m.Description)
            .MaximumLength(200);

        RuleFor(m => m.Manufacturer)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(m => m.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category is required.");
    }
}