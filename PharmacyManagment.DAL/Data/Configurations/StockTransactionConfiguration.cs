using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.Property(t => t.TransactionType).IsRequired();
        builder.Property(t => t.Quantity).IsRequired();
        builder.Property(t => t.Notes).HasMaxLength(500);
        builder.Property(t => t.UserId).HasMaxLength(450);
        builder.HasOne(t => t.Medicine).WithMany().HasForeignKey(t => t.MedicineId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.MedicineBatch).WithMany().HasForeignKey(t => t.MedicineBatchId).OnDelete(DeleteBehavior.SetNull);
    }
}
