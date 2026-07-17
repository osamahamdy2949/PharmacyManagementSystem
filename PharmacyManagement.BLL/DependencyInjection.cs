using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PharmacyManagement.BLL.Mapping;
using PharmacyManagement.BLL.Services.Classes;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.Validators;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;

namespace PharmacyManagement.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        services.AddScoped<IValidator<MedicineViewModel>, MedicineViewModelValidator>();
        services.AddScoped<MedicineEditViewModelValidator>();
        services.AddValidatorsFromAssemblyContaining<SupplierViewModelValidator>();
        // Purchase/Sales POS use service-layer validation

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IMedicineService, MedicineService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<ISalesReturnService, SalesReturnService>();
        services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddSingleton<IDocumentGeneratorService, DocumentGeneratorService>();

        return services;
    }
}
