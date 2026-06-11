using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
    {
        builder.Property(p => p.SubTotal).HasColumnType("decimal(10,2)");
        builder.Property(p => p.VatAmount).HasColumnType("decimal(10,2)");
        builder.Property(p => p.TotalAmount).HasColumnType("decimal(10,2)");
        builder.Property(p => p.CreatedByUserId).HasMaxLength(450);
        builder.HasOne(p => p.Supplier).WithMany(s => s.PurchaseInvoices)
               .HasForeignKey(p => p.SupplierId).OnDelete(DeleteBehavior.Restrict);
    }
}
