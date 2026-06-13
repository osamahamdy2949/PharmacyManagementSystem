using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.AmountPaid).HasColumnType("decimal(10,2)");
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.Property(p => p.RecordedByUserId).HasMaxLength(450);

        builder.HasOne(p => p.Customer)
               .WithMany(c => c.Payments)
               .HasForeignKey(p => p.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.SalesInvoice)
               .WithMany(s => s.Payments)
               .HasForeignKey(p => p.SalesInvoiceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.CustomerId);
        builder.HasIndex(p => p.SalesInvoiceId);
        builder.HasIndex(p => p.PaymentDate);
    }
}
