using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.Property(r => r.ReturnDate).IsRequired();
        builder.Property(r => r.Reason).HasMaxLength(500);
        builder.Property(r => r.RefundAmount).HasColumnType("decimal(10,2)");
        builder.Property(r => r.CreatedByUserId).HasMaxLength(450);
        builder.HasOne(r => r.SalesInvoice).WithMany(i => i.SalesReturns).HasForeignKey(r => r.SalesInvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Customer).WithMany().HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
