using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.DAL.Data.Entities;

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
        var auditEntries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
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
                case EntityState.Deleted when entry.Entity is not AuditLog and not Notification:
                    if (entry.Entity is Medicine or Category or Supplier or Customer or MedicineBatch)
                    {
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
                if (entityName is nameof(AuditLog) or nameof(Notification) or nameof(StockTransaction))
                    continue;

                auditEntries.Add(new AuditLog
                {
                    UserId = CurrentUserId,
                    Action = entry.State.ToString(),
                    Entity = entityName,
                    EntityId = entry.Entity.Id == 0 ? null : entry.Entity.Id,
                    NewValue = entry.State == EntityState.Deleted ? null : entry.Entity.ToString()
                });
            }
        }

        if (auditEntries.Count > 0)
            AuditLogs.AddRange(auditEntries);

        return await base.SaveChangesAsync(cancellationToken);
    }
}
