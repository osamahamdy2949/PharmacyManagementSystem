using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;
using PharmacyManagement.BLL.ViewModels.SalesReturnViewModels;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class SalesReturnService : ISalesReturnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IStockService _stockService;
    private readonly ICurrentUserService _currentUser;

    public SalesReturnService(IUnitOfWork unitOfWork, IMapper mapper, IStockService stockService, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _stockService = stockService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SalesReturnListItemViewModel>> GetAllAsync()
    {
        var items = await _unitOfWork.GetRepository<SalesReturn>().Query()
            .Include(r => r.Customer)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync();

        return items.Select(r => new SalesReturnListItemViewModel
        {
            Id = r.Id,
            SalesInvoiceId = r.SalesInvoiceId,
            CustomerName = r.Customer.Name,
            ReturnDate = r.ReturnDate,
            TotalAmount = r.RefundAmount,
            Reason = r.Reason
        }).ToList();
    }

    public async Task<SalesReturnViewModel?> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.GetRepository<SalesReturn>().Query()
            .Include(r => r.Customer)
            .Include(r => r.Items).ThenInclude(i => i.Medicine)
            .Include(r => r.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (item == null) return null;

        return new SalesReturnViewModel
        {
            Id = item.Id,
            SalesInvoiceId = item.SalesInvoiceId,
            CustomerId = item.CustomerId,
            CustomerName = item.Customer.Name,
            ReturnDate = item.ReturnDate,
            Reason = item.Reason,
            TotalAmount = item.RefundAmount,
            Items = item.Items.Select(i => new SalesReturnItemViewModel
            {
                MedicineBatchId = i.MedicineBatchId,
                MedicineId = i.MedicineId,
                MedicineName = i.Medicine.TradeName,
                BatchNumber = i.MedicineBatch.BatchNumber,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }

    public async Task<CreateSalesReturnViewModel?> GetCreateModelFromInvoiceAsync(int invoiceId)
    {
        var invoice = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Include(s => s.Items).ThenInclude(i => i.Medicine)
            .Include(s => s.Items).ThenInclude(i => i.MedicineBatch)
            .FirstOrDefaultAsync(s => s.Id == invoiceId);

        if (invoice == null) return null;

        return new CreateSalesReturnViewModel
        {
            SalesInvoiceId = invoice.Id,
            ReturnDate = DateTime.Today,
            Items = invoice.Items.Select(i => new SalesReturnItemViewModel
            {
                MedicineBatchId = i.MedicineBatchId,
                MedicineId = i.MedicineId,
                MedicineName = i.Medicine.TradeName,
                BatchNumber = i.MedicineBatch.BatchNumber,
                Quantity = 0,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }

    public async Task<ServiceResult<int>> CreateAsync(CreateSalesReturnViewModel model)
    {
        if (model.SalesInvoiceId <= 0)
            return ServiceResult<int>.Fail("Original invoice is required.");

        var invoice = await _unitOfWork.GetRepository<SalesInvoice>().Query()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == model.SalesInvoiceId);

        if (invoice == null)
            return ServiceResult<int>.Fail("Original invoice not found.");

        var returnLines = model.Items.Where(i => i.Quantity > 0).ToList();
        if (returnLines.Count == 0)
            return ServiceResult<int>.Fail("At least one returned item is required.");

        foreach (var line in returnLines)
        {
            var sold = invoice.Items.FirstOrDefault(i =>
                i.MedicineBatchId == line.MedicineBatchId && i.MedicineId == line.MedicineId);

            if (sold == null)
                return ServiceResult<int>.Fail($"Batch line was not on the original invoice.");

            var alreadyReturned = await _unitOfWork.GetRepository<SalesReturnItem>().Query()
                .Where(ri => ri.SalesReturn.SalesInvoiceId == invoice.Id && ri.MedicineBatchId == line.MedicineBatchId)
                .SumAsync(ri => ri.Quantity);

            if (line.Quantity + alreadyReturned > sold.Quantity)
                return ServiceResult<int>.Fail($"Cannot return more than sold quantity for {line.MedicineName ?? "medicine"}.");
        }

        var returnItems = returnLines.Select(l => new SalesReturnItem
        {
            MedicineBatchId = l.MedicineBatchId,
            MedicineId = l.MedicineId,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            Reason = model.Reason
        }).ToList();

        var salesReturn = new SalesReturn
        {
            SalesInvoiceId = invoice.Id,
            CustomerId = invoice.CustomerId,
            ReturnDate = model.ReturnDate.Date,
            Reason = model.Reason,
            RefundAmount = returnItems.Sum(i => i.Quantity * i.UnitPrice),
            CreatedByUserId = _currentUser.UserId,
            Items = returnItems
        };

        await using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
        try
        {
            _unitOfWork.GetRepository<SalesReturn>().Add(salesReturn);
            await _unitOfWork.SaveChangesAsync();

            foreach (var line in returnItems)
                await _stockService.RestoreToBatchAsync(line.MedicineBatchId, line.Quantity, salesReturn.Id);

            var customer = await _unitOfWork.GetRepository<Customer>().GetByIdAsync(invoice.CustomerId, tracking: true);
            if (customer != null)
                customer.TotalSpent = Math.Max(0, customer.TotalSpent - salesReturn.RefundAmount);

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return ServiceResult<int>.Ok(salesReturn.Id);
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult<int>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult<int>.Fail(ex.Message);
        }
    }

    public async Task<int> GetReturnCountAsync() =>
        await _unitOfWork.GetRepository<SalesReturn>().Query().CountAsync();
}
