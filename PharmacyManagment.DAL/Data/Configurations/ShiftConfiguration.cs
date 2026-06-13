using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.Property(s => s.UserId).IsRequired().HasMaxLength(450);
        
        builder.Property(s => s.ClosingCash).HasColumnType("decimal(10,2)");
        builder.Property(s => s.CashDifference).HasColumnType("decimal(10,2)");
        
        builder.Property(s => s.TotalSales).HasColumnType("decimal(10,2)");
        builder.Property(s => s.CashSales).HasColumnType("decimal(10,2)");
        builder.Property(s => s.CreditSales).HasColumnType("decimal(10,2)");
        builder.Property(s => s.TotalPurchases).HasColumnType("decimal(10,2)");
        builder.Property(s => s.TotalPaymentsReceived).HasColumnType("decimal(10,2)");
        builder.Property(s => s.NetCash).HasColumnType("decimal(10,2)");
        
        builder.Property(s => s.Notes).HasMaxLength(500);

        builder.HasOne(s => s.User)
               .WithMany()
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.StartTime);
    }
}
