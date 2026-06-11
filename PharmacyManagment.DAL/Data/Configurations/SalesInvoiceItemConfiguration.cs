using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class SalesInvoiceItemConfiguration : IEntityTypeConfiguration<SalesInvoiceItem>
{
    public void Configure(EntityTypeBuilder<SalesInvoiceItem> builder)
    {
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
        builder.Property(i => i.Discount).HasColumnType("decimal(10,2)");
        builder.HasOne(i => i.SalesInvoice)
               .WithMany(s => s.Items)
               .HasForeignKey(i => i.SalesInvoiceId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Medicine)
               .WithMany(m => m.SalesInvoiceItems)
               .HasForeignKey(i => i.MedicineId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.MedicineBatch)
               .WithMany()
               .HasForeignKey(i => i.MedicineBatchId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
