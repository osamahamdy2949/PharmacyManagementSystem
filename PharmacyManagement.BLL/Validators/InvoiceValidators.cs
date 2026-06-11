using FluentValidation;
using PharmacyManagement.BLL.ViewModels.PurchaseInvoiceViewModels;
using PharmacyManagement.BLL.ViewModels.SalesInvoiceViewModels;

namespace PharmacyManagement.BLL.Validators;

public class CreatePurchaseInvoiceViewModelValidator : AbstractValidator<CreatePurchaseInvoiceViewModel>
{
    public CreatePurchaseInvoiceViewModelValidator()
    {
        RuleFor(p => p.SupplierId).GreaterThan(0).WithMessage("Supplier is required.");
        RuleFor(p => p.InvoiceDate).NotEmpty().WithMessage("Invoice date is required.");
        RuleFor(p => p)
            .Must(p => p.Items.Any(i => i.MedicineId > 0 && i.Quantity > 0 && i.DoseValue > 0 && i.DoseUnit.HasValue))
            .WithMessage("At least one line item must be added.");

        RuleForEach(p => p.Items).ChildRules(item =>
        {
            item.When(i => i.MedicineId > 0, () =>
            {
                item.RuleFor(i => i.MedicineId).GreaterThan(0).WithMessage("Select a medicine.");
                item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
                item.RuleFor(i => i.DoseValue).GreaterThan(0).WithMessage("Dose value is required.");
                item.RuleFor(i => i.DoseUnit).NotNull().WithMessage("Dose unit is required.");
            });
        });
    }
}

public class CreateSalesInvoiceViewModelValidator : AbstractValidator<CreateSalesInvoiceViewModel>
{
    public CreateSalesInvoiceViewModelValidator()
    {
        RuleFor(s => s.CustomerId).GreaterThan(0).WithMessage("Customer is required.");
        RuleFor(s => s.InvoiceDate).NotEmpty().WithMessage("Invoice date is required.");
        RuleFor(s => s)
            .Must(s => s.Items.Any(i => i.MedicineId > 0 && i.Quantity > 0 && i.UnitPrice > 0))
            .WithMessage("At least one line item must be added.");

        RuleForEach(s => s.Items).ChildRules(item =>
        {
            item.When(i => i.MedicineId > 0 || i.Quantity > 0 || i.UnitPrice > 0, () =>
            {
                item.RuleFor(i => i.MedicineId).GreaterThan(0).WithMessage("Select a medicine.");
                item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
                item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("Unit price must be greater than zero.");
            });
        });
    }
}
