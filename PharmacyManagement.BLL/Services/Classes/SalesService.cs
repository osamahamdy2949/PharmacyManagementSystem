using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.PosViewModels;
using PharmacyManagement.BLL.ViewModels.SalesInvoiceViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class SalesService : ISalesService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStockService _stockService;
    private readonly ICustomerService _customerService;
    private readonly IVatService _vatService;
    private readonly ICurrentUserService _currentUser;

    public SalesService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IStockService stockService,
        ICustomerService customerService,
        IVatService vatService,
        ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _stockService = stockService;
        _customerService = customerService;
        _vatService = vatService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SalesInvoiceViewModel>> GetAllAsync()
    {
        var items = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Include(s => s.Customer)
            .Include(s => s.Items).ThenInclude(i => i.Medicine)
            .Include(s => s.Items).ThenInclude(i => i.MedicineBatch)
            .OrderByDescending(s => s.InvoiceDate)
            .ToListAsync();

        var viewModels = _mapper.Map<List<SalesInvoiceViewModel>>(items);

        var userIds = items.Select(i => i.CreatedByUserId).Where(id => id != null).Distinct().ToList();
        if (userIds.Any())
        {
            var users = await _unitOfWork.Context.Set<ApplicationUser>()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].CreatedByUserId != null && users.TryGetValue(items[i].CreatedByUserId!, out var fullName))
                {
                    viewModels[i].CreatedByUserName = fullName;
                }
                else
                {
                    viewModels[i].CreatedByUserName = "System / Unknown";
                }
            }
        }

        return viewModels;
    }

    public async Task<SalesInvoiceViewModel?> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Include(s => s.Customer)
            .Include(s => s.Items).ThenInclude(i => i.Medicine)
            .Include(s => s.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(s => s.Id == id);
        return item == null ? null : _mapper.Map<SalesInvoiceViewModel>(item);
    }

    public async Task<IReadOnlyList<MedicineSaleLookupViewModel>> SearchMedicinesForSaleAsync(string? query)
    {
        var today = DateTime.Today;
        var batchQuery = _unitOfWork.GetRepository<MedicineBatch>().Query()
            .Include(b => b.Medicine)
            .Where(b => b.IsActive && b.ExpiryDate > today && b.CurrentQuantity > 0);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim().ToLower();
            batchQuery = batchQuery.Where(b =>
                b.Medicine.TradeName.ToLower().Contains(term) ||
                b.Medicine.ScientificName.ToLower().Contains(term) ||
                b.Dose.ToLower().Contains(term) ||
                (b.Barcode != null && b.Barcode.ToLower() == term));
        }

        var batches = await batchQuery
            .OrderBy(b => b.Medicine.TradeName)
            .ThenBy(b => b.Dose)
            .ThenBy(b => b.ExpiryDate)
            .Take(100)
            .ToListAsync();

        var grouped = batches
            .GroupBy(b => new { b.MedicineId, b.Dose })
            .Take(30);

        var results = new List<MedicineSaleLookupViewModel>();
        foreach (var group in grouped)
        {
            var first = group.First();
            var medicine = first.Medicine;
            var vm = new MedicineSaleLookupViewModel
            {
                Id = medicine.Id,
                SerialNumber = medicine.SerialNumber,
                TradeName = medicine.TradeName,
                ScientificName = medicine.ScientificName,
                MedicineForm = medicine.MedicineForm.ToString(),
                Dose = first.Dose,
                SaleUnit = first.SaleUnit.ToString(),
                PurchaseUnit = first.PurchaseUnit.ToString(),
                UnitsPerPurchaseUnit = first.UnitsPerPurchaseUnit,
                SellingPrice = first.SellingPrice,
                QuantityInStock = group.Sum(b => b.CurrentQuantity),
                Batches = group.Select(b => new PosBatchOptionViewModel
                {
                    BatchId = b.Id,
                    BatchNumber = b.BatchNumber,
                    ExpiryDate = b.ExpiryDate,
                    QuantityInStock = b.CurrentQuantity,
                    SellingPrice = b.SellingPrice
                }).ToList()
            };
            results.Add(vm);
        }

        return results;
    }

    public async Task<ServiceResult<PosCheckoutResultViewModel>> CheckoutPosAsync(PosCheckoutViewModel model)
    {
        if (model.Items == null || model.Items.Count == 0)
            return ServiceResult<PosCheckoutResultViewModel>.Fail("Cannot checkout with an empty invoice.");

        int customerId = model.CustomerId;
        if (customerId <= 0)
        {
            if (string.IsNullOrWhiteSpace(model.CustomerName))
                return ServiceResult<PosCheckoutResultViewModel>.Fail("Customer name is required.");

            var customerResult = await _customerService.GetOrCreateByNamePhoneAsync(model.CustomerName, model.CustomerPhone);
            if (!customerResult.Success)
                return ServiceResult<PosCheckoutResultViewModel>.Fail(customerResult.ErrorMessage!);

            customerId = customerResult.Data;
        }
        else if (!await _unitOfWork.GetRepository<Customer>().AnyAsync(c => c.Id == customerId))
        {
            return ServiceResult<PosCheckoutResultViewModel>.Fail("Customer does not exist.");
        }

        var cartLines = model.Items.Where(i => i.Quantity > 0).ToList();
        foreach (var line in cartLines)
        {
            var medicine = await _unitOfWork.GetRepository<Medicine>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == line.MedicineId);

            if (medicine == null)
                return ServiceResult<PosCheckoutResultViewModel>.Fail($"Medicine id {line.MedicineId} not found.");

            var dose = line.Dose?.Trim() ?? string.Empty;
            var availableStock = string.IsNullOrEmpty(dose)
                ? await _stockService.GetAvailableStockAsync(medicine.Id)
                : await _stockService.GetAvailableStockAsync(medicine.Id, dose);

            if (line.Quantity > availableStock)
                return ServiceResult<PosCheckoutResultViewModel>.Fail(
                    $"Cannot sell {line.Quantity} unit(s) of {medicine.TradeName}. Only {availableStock} in stock.");
        }

        await using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
        try
        {
            var invoiceLines = new List<SalesInvoiceItem>();

            foreach (var line in cartLines)
            {
                var medicine = await _unitOfWork.GetRepository<Medicine>().Query()
                    .AsNoTracking()
                    .FirstAsync(m => m.Id == line.MedicineId);

                var deductions = line.BatchId.HasValue
                    ? await _stockService.DeductStockFromBatchAsync(line.BatchId.Value, line.Quantity)
                    : await _stockService.DeductStockFefoAsync(
                        medicine.Id, line.Quantity, preferredBatchId: null, dose: line.Dose?.Trim());

                foreach (var d in deductions)
                {
                    invoiceLines.Add(new SalesInvoiceItem
                    {
                        MedicineId = medicine.Id,
                        MedicineBatchId = d.BatchId,
                        Quantity = d.Quantity,
                        UnitPrice = line.UnitPrice > 0 ? line.UnitPrice : d.UnitPrice,
                        Discount = line.Discount
                    });
                }
            }

            var subTotal = invoiceLines.Sum(i => _vatService.LineTotal(i.UnitPrice, i.Quantity, i.Discount));
            var (sub, vat, total) = _vatService.Calculate(subTotal);

            var invoice = new SalesInvoice
            {
                CustomerId = customerId,
                InvoiceDate = DateTime.Now,
                DoctorName = model.DoctorName?.Trim(),
                SubTotal = sub,
                VatAmount = vat,
                TotalAmount = total,
                CreatedByUserId = _currentUser.UserId,
                Items = invoiceLines
            };

            _unitOfWork.GetRepository<SalesInvoice>().Add(invoice);
            await _unitOfWork.SaveChangesAsync();

            var customer = await _unitOfWork.GetRepository<Customer>().GetByIdAsync(customerId, tracking: true);
            if (customer != null)
                customer.TotalSpent += total;

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<PosCheckoutResultViewModel>.Ok(new PosCheckoutResultViewModel
            {
                InvoiceId = invoice.Id,
                SubTotal = sub,
                VatAmount = vat,
                TotalAmount = total,
                ItemCount = invoiceLines.Count
            });
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult<PosCheckoutResultViewModel>.Fail(ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
