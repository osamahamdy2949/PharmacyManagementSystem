using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class SalesInvoiceConfiguration : IEntityTypeConfiguration<SalesInvoice>
{
    public void Configure(EntityTypeBuilder<SalesInvoice> builder)
    {
        builder.Property(s => s.SubTotal).HasColumnType("decimal(10,2)");
        builder.Property(s => s.VatAmount).HasColumnType("decimal(10,2)");
        builder.Property(s => s.TotalAmount).HasColumnType("decimal(10,2)");
        builder.Property(s => s.DoctorName).HasMaxLength(100);
        builder.Property(s => s.CreatedByUserId).HasMaxLength(450);
        builder.HasOne(s => s.Customer)
               .WithMany(c => c.SalesInvoices)
               .HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}
