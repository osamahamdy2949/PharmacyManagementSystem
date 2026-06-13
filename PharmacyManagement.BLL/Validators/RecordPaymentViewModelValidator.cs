using FluentValidation;
using PharmacyManagement.BLL.ViewModels.PaymentViewModels;

namespace PharmacyManagement.BLL.Validators;

public class RecordPaymentViewModelValidator : AbstractValidator<RecordPaymentViewModel>
{
    public RecordPaymentViewModelValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Please select a customer.");

        RuleFor(x => x.AmountPaid)
            .GreaterThan(0).WithMessage("Amount paid must be greater than zero.")
            .LessThanOrEqualTo(x => x.CustomerTotalDebt).When(x => x.CustomerTotalDebt > 0 && !x.SalesInvoiceId.HasValue)
            .WithMessage("Amount paid cannot exceed total debt.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("Payment date cannot be in the future.");
    }
}
