using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.DAL.Data.Entities;
using System.Text.Json;

namespace PharmacyManagement.DAL.Data.DbContexts;

public class PharmacyDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IAuditUserProvider? _auditUserProvider;

    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options) { }

    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options, IAuditUserProvider auditUserProvider) : base(options)
    {
        _auditUserProvider = auditUserProvider;
    }

    private string? CurrentUserId => _auditUserProvider?.UserId;

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
    public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<MedicineBatch> MedicineBatches => Set<MedicineBatch>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<SalesReturnItem> SalesReturnItems => Set<SalesReturnItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PharmacyDbContext).Assembly);

        ApplySoftDeleteFilter<Category>(modelBuilder);
        ApplySoftDeleteFilter<Medicine>(modelBuilder);
        ApplySoftDeleteFilter<Supplier>(modelBuilder);
        ApplySoftDeleteFilter<Customer>(modelBuilder);
        ApplySoftDeleteFilter<MedicineBatch>(modelBuilder);
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity =>
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var auditEntries = new List<AuditEntryInfo>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            var auditAction = entry.State.ToString();

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    if (!string.IsNullOrEmpty(CurrentUserId))
                        entry.Entity.CreatedBy = CurrentUserId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    if (!string.IsNullOrEmpty(CurrentUserId))
                        entry.Entity.UpdatedBy = CurrentUserId;
                    break;
                case EntityState.Deleted when entry.Entity is not AuditLog and not Notification and not Payment and not Shift and not UserActivity:
                    if (entry.Entity is Medicine or Category or Supplier or Customer or MedicineBatch)
                    {
                        auditAction = EntityState.Deleted.ToString();
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeletedBy = CurrentUserId;
                    }
                    break;
            }

            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            {
                var entityName = entry.Entity.GetType().Name;
                if (entityName is nameof(AuditLog) or nameof(Notification) or nameof(StockTransaction) or nameof(Payment) or nameof(Shift) or nameof(UserActivity))
                    continue;

                auditEntries.Add(new AuditEntryInfo 
                { 
                    Entry = entry, 
                    Action = auditAction, 
                    EntityName = entityName 
                });
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            var logs = auditEntries.Select(a => new AuditLog
            {
                UserId = CurrentUserId,
                Action = a.Action,
                Entity = a.EntityName,
                EntityId = a.Entry.Entity.Id == 0 ? null : a.Entry.Entity.Id,
                OldValues = a.Action == "Added" ? null : JsonSerializer.Serialize(a.Entry.OriginalValues.ToObject()),
                NewValues = a.Action == "Deleted" ? null : JsonSerializer.Serialize(a.Entry.CurrentValues.ToObject()),
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = CurrentUserId
            }).ToList();

            AuditLogs.AddRange(logs);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private class AuditEntryInfo
    {
        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> Entry { get; set; } = null!;
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
    }
}
