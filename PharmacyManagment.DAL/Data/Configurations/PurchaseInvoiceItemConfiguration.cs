using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class PurchaseInvoiceItemConfiguration : IEntityTypeConfiguration<PurchaseInvoiceItem>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceItem> builder)
    {
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
        builder.Property(i => i.SellingPrice).HasColumnType("decimal(10,2)");
        builder.Property(i => i.BatchNumber).IsRequired().HasMaxLength(50);
        
        builder.HasOne(i => i.PurchaseInvoice)
               .WithMany(p => p.Items)
               .HasForeignKey(i => i.PurchaseInvoiceId).OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(i => i.MedicineBatch)
               .WithMany()
               .HasForeignKey(i => i.MedicineBatchId)
               .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(i => i.Medicine)
               .WithMany(m => m.PurchaseInvoiceItems)
               .HasForeignKey(i => i.MedicineId).OnDelete(DeleteBehavior.Restrict);
    }
}
