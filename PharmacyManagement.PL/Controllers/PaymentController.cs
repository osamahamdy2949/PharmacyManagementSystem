using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.PaymentViewModels;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.PL.Helpers;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.AllStaff)]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly ICustomerService _customerService;
    private readonly ISalesService _salesService;

    public PaymentController(
        IPaymentService paymentService, 
        ICustomerService customerService,
        ISalesService salesService)
    {
        _paymentService = paymentService;
        _customerService = customerService;
        _salesService = salesService;
    }

    public async Task<IActionResult> Index(PaymentHistoryFilterViewModel filter)
    {
        ViewBag.Customers = await _customerService.GetAllAsync();
        var payments = await _paymentService.GetPaymentHistoryAsync(filter);
        ViewBag.Filter = filter;
        return View(payments);
    }

    public async Task<IActionResult> RecordPayment(int? customerId, int? invoiceId)
    {
        var model = new RecordPaymentViewModel();

        if (customerId.HasValue)
        {
            var customer = await _customerService.GetCustomerProfileAsync(customerId.Value);
            if (customer != null)
            {
                model.CustomerId = customer.Id;
                model.CustomerName = customer.Name;
                model.CustomerTotalDebt = customer.TotalDebt;
            }
        }
        else if (invoiceId.HasValue)
        {
            var invoice = await _salesService.GetByIdAsync(invoiceId.Value);
            if (invoice != null)
            {
                model.SalesInvoiceId = invoice.Id;
                model.CustomerId = invoice.CustomerId;
                model.CustomerName = invoice.CustomerName ?? string.Empty;
                model.CustomerTotalDebt = invoice.RemainingAmount;
                model.AmountPaid = invoice.RemainingAmount;
            }
        }
        else
        {
            // Allow searching customers
            var customers = await _customerService.GetAllAsync();
            ViewBag.CustomersWithDebt = customers.Where(c => c.RemainingBalance > 0).ToList();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(RecordPaymentViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _paymentService.RecordPaymentAsync(model);
        if (!result.Success)
        {
            ModelState.AddServiceErrors(result);
            return View(model);
        }

        TempData["Success"] = "Payment recorded successfully.";
        
        if (model.SalesInvoiceId.HasValue)
            return RedirectToAction("Details", "SalesInvoice", new { id = model.SalesInvoiceId.Value });
            
        return RedirectToAction("Profile", "Customer", new { id = model.CustomerId });
    }
}
