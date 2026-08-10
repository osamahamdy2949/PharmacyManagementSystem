using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using PharmacyManagement.DAL.Repositories.Interfaces;
using PharmacyManagement.DAL.Repositories.Models;

namespace PharmacyManagement.DAL.Repositories.Classes;

public class DashboardRepository : IDashboardRepository
{
    private readonly PharmacyDbContext _context;

    public DashboardRepository(PharmacyDbContext context) => _context = context;

    public async Task<DashboardData> GetDashboardDataAsync(
        DateTime today,
        DateTime thisMonthStart,
        DateTime nextMonthStart,
        DateTime inThreeMonths,
        CancellationToken ct = default)
    {
        var tomorrow = today.AddDays(1);

        var activities = new List<RecentActivityData>();

        activities.AddRange(await _context.Set<SalesInvoice>()
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Take(3)
            .Select(s => new RecentActivityData
            {
                ActivityType = "Sale",
                Description = $"Sale Invoice #{s.Id}",
                Time = s.CreatedAt,
                Amount = s.TotalAmount,
                Icon = "bi-cart-check",
                Color = "text-success bg-success-subtle"
            })
            .ToListAsync(ct));

        activities.AddRange(await _context.Set<PurchaseInvoice>()
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(2)
            .Select(p => new RecentActivityData
            {
                ActivityType = "Purchase",
                Description = $"Purchase Invoice #{p.Id}",
                Time = p.CreatedAt,
                Amount = p.TotalAmount,
                Icon = "bi-bag-plus",
                Color = "text-danger bg-danger-subtle"
            })
            .ToListAsync(ct));

        activities.AddRange(await _context.Set<Payment>()
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(2)
            .Select(p => new RecentActivityData
            {
                ActivityType = "Payment",
                Description = $"Payment from {p.Customer.Name}",
                Time = p.CreatedAt,
                Amount = p.AmountPaid,
                Icon = "bi-cash-coin",
                Color = "text-info bg-info-subtle"
            })
            .ToListAsync(ct));

        return new DashboardData
        {
            TodaySales = await _context.Set<SalesInvoice>().AsNoTracking()
                .Where(s => s.InvoiceDate >= today && s.InvoiceDate < tomorrow)
                .SumAsync(s => s.TotalAmount, ct),
            TodayPurchases = await _context.Set<PurchaseInvoice>().AsNoTracking()
                .Where(p => p.InvoiceDate >= today && p.InvoiceDate < tomorrow)
                .SumAsync(p => p.TotalAmount, ct),
            MonthlySales = await _context.Set<SalesInvoice>().AsNoTracking()
                .Where(s => s.InvoiceDate >= thisMonthStart && s.InvoiceDate < nextMonthStart)
                .SumAsync(s => s.TotalAmount, ct),
            MonthlyPurchases = await _context.Set<PurchaseInvoice>().AsNoTracking()
                .Where(p => p.InvoiceDate >= thisMonthStart && p.InvoiceDate < nextMonthStart)
                .SumAsync(p => p.TotalAmount, ct),
            TotalCustomers = await _context.Set<Customer>().AsNoTracking().CountAsync(ct),
            OutstandingDebt = await _context.Set<Customer>().AsNoTracking().SumAsync(c => c.RemainingBalance, ct),
            TotalProducts = await _context.Set<Medicine>().AsNoTracking().CountAsync(ct),
            LowStockItems = await _context.Set<Medicine>().AsNoTracking()
                .CountAsync(m => !m.MedicineBatches.Any(b => b.ExpiryDate > today && b.CurrentQuantity > 0), ct),
            NearLowStockItems = await _context.Set<Medicine>().AsNoTracking()
                .CountAsync(m =>
                    m.MedicineBatches.Where(b => b.ExpiryDate > today).Sum(b => b.CurrentQuantity) > 0 &&
                    m.MedicineBatches.Where(b => b.ExpiryDate > today).Sum(b => b.CurrentQuantity) <= m.MinStockLevel, ct),
            ExpiringMedicines = await _context.Set<MedicineBatch>().AsNoTracking()
                .CountAsync(b => b.ExpiryDate < today && b.CurrentQuantity > 0, ct),
            NearExpiringMedicines = await _context.Set<MedicineBatch>().AsNoTracking()
                .CountAsync(b => b.ExpiryDate >= today && b.ExpiryDate <= inThreeMonths && b.CurrentQuantity > 0, ct),
            RecentSalesByDate = await _context.Set<SalesInvoice>().AsNoTracking()
                .Where(s => s.InvoiceDate >= today.AddDays(-6) && s.InvoiceDate < tomorrow)
                .GroupBy(s => s.InvoiceDate.Date)
                .Select(g => new { Date = g.Key, Amount = g.Sum(s => s.TotalAmount) })
                .ToDictionaryAsync(g => g.Date, g => g.Amount, ct),
            RecentActivities = activities.OrderByDescending(a => a.Time).Take(6).ToList()
        };
    }

    public async Task<MainDashboardData> GetMainDashboardDataAsync(
        DateTime today,
        DateTime tomorrow,
        DateTime monthStart,
        DateTime nearExpiry,
        DateTime todayUtc,
        CancellationToken ct = default)
    {
        var batches = await _context.Set<MedicineBatch>()
            .Include(b => b.Medicine)
            .Where(b => b.CurrentQuantity > 0)
            .ToListAsync(ct);

        return new MainDashboardData
        {
            TotalMedicines = await _context.Set<Medicine>().CountAsync(ct),
            TotalCategories = await _context.Set<Category>().CountAsync(ct),
            TotalSuppliers = await _context.Set<Supplier>().CountAsync(ct),
            TotalCustomers = await _context.Set<Customer>().CountAsync(ct),
            LowStockCount = await _context.Set<Medicine>()
                .CountAsync(m => m.MedicineBatches.Sum(b => (b.ExpiryDate > today && b.CurrentQuantity > 0) ? b.CurrentQuantity : 0) < m.MinStockLevel, ct),
            CustomersToday = await _context.Set<Customer>().CountAsync(c => c.CreatedAt >= todayUtc, ct),
            ActiveUsers = await _context.Set<ApplicationUser>().CountAsync(ct),
            ExpiredCount = await _context.Set<MedicineBatch>()
                .CountAsync(b => b.ExpiryDate < today && b.CurrentQuantity > 0, ct),
            NearExpiryCount = await _context.Set<MedicineBatch>()
                .CountAsync(b => b.ExpiryDate >= today && b.ExpiryDate <= nearExpiry && b.CurrentQuantity > 0, ct),
            TodaySales = await _context.Set<SalesInvoice>()
                .Where(s => s.InvoiceDate >= today && s.InvoiceDate < tomorrow)
                .SumAsync(s => s.TotalAmount, ct),
            MonthlySales = await _context.Set<SalesInvoice>()
                .Where(s => s.InvoiceDate >= monthStart && s.InvoiceDate < tomorrow)
                .SumAsync(s => s.TotalAmount, ct),
            TodayPurchases = await _context.Set<PurchaseInvoice>()
                .Where(p => p.InvoiceDate >= today && p.InvoiceDate < tomorrow)
                .SumAsync(p => p.TotalAmount, ct),
            MonthlyPurchases = await _context.Set<PurchaseInvoice>()
                .Where(p => p.InvoiceDate >= monthStart && p.InvoiceDate < tomorrow)
                .SumAsync(p => p.TotalAmount, ct),
            InventoryTotalValue = batches.Sum(b =>
                b.CurrentQuantity * b.PurchasePrice / Math.Max(1, b.Medicine.UnitsPerPurchaseUnit))
        };
    }
}
