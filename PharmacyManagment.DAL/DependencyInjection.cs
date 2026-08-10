using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Repositories.Classes;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PharmacyDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IMedicineRepository, MedicineRepository>();
        services.AddScoped<IMedicineBatchRepository, MedicineBatchRepository>();
        services.AddScoped<ISearchRepository, SearchRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<ISalesInvoiceRepository, SalesInvoiceRepository>();
        services.AddScoped<IPurchaseInvoiceRepository, PurchaseInvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<ISalesReturnRepository, SalesReturnRepository>();
        services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IDataTransactionManager, DataTransactionManager>();

        return services;
    }
}
