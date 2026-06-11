using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class SalesReturnItemConfiguration : IEntityTypeConfiguration<SalesReturnItem>
{
    public void Configure(EntityTypeBuilder<SalesReturnItem> builder)
    {
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
        builder.Property(i => i.Reason).HasMaxLength(500);
        builder.HasOne(i => i.SalesReturn).WithMany(r => r.Items).HasForeignKey(i => i.SalesReturnId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.MedicineBatch).WithMany().HasForeignKey(i => i.MedicineBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Medicine).WithMany().HasForeignKey(i => i.MedicineId).OnDelete(DeleteBehavior.Restrict);
    }
}
