using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.ViewModels.PosViewModels;
using PharmacyManagement.BLL.ViewModels.SalesInvoiceViewModels;

namespace PharmacyManagement.BLL.Services.Interfaces;

public interface ISalesService
{
    Task<IReadOnlyList<SalesInvoiceViewModel>> GetAllAsync();
    Task<SalesInvoiceViewModel?> GetByIdAsync(int id);
    Task<IReadOnlyList<MedicineSaleLookupViewModel>> SearchMedicinesForSaleAsync(string? query);
    Task<ServiceResult<PosCheckoutResultViewModel>> CheckoutPosAsync(PosCheckoutViewModel model);
}
