using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.Property(r => r.ReturnDate).IsRequired();
        builder.Property(r => r.Notes).HasMaxLength(500);
        builder.Property(r => r.TotalAmount).HasColumnType("decimal(10,2)");
        builder.Property(r => r.CreatedByUserId).HasMaxLength(450);
        builder.HasOne(r => r.PurchaseInvoice).WithMany(i => i.PurchaseReturns).HasForeignKey(r => r.PurchaseInvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Supplier).WithMany(s => s.PurchaseReturns).HasForeignKey(r => r.SupplierId).OnDelete(DeleteBehavior.Restrict);
    }
}
