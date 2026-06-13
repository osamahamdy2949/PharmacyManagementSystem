using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.PaymentViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface IPaymentService
{
    Task<ServiceResult> RecordPaymentAsync(RecordPaymentViewModel model);
    Task<IReadOnlyList<PaymentViewModel>> GetPaymentHistoryAsync(PaymentHistoryFilterViewModel filter);
    Task<IReadOnlyList<PaymentViewModel>> GetPaymentsByCustomerAsync(int customerId);
    Task<IReadOnlyList<PaymentViewModel>> GetPaymentsByInvoiceAsync(int invoiceId);
    Task<PaymentViewModel?> GetByIdAsync(int id);
}
