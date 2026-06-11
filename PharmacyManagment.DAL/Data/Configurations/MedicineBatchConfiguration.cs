using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class MedicineBatchConfiguration : IEntityTypeConfiguration<MedicineBatch>
{
    public void Configure(EntityTypeBuilder<MedicineBatch> builder)
    {
        builder.ToTable("MedicineBatches", t => t.HasCheckConstraint("CK_MedicineBatches_CurrentQuantity", "[CurrentQuantity] >= 0"));

        builder.Property(b => b.Sku).IsRequired().HasMaxLength(30);
        builder.Property(b => b.Dose).HasMaxLength(50);
        builder.Property(b => b.BatchNumber).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Barcode).HasMaxLength(50);
        builder.Property(b => b.PurchasePrice).HasColumnType("decimal(10,2)");
        builder.Property(b => b.SellingPrice).HasColumnType("decimal(10,2)");
        builder.Property(b => b.IsActive).HasDefaultValue(false);
        builder.Property(b => b.RowVersion).IsRowVersion();
        builder.HasIndex(b => b.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");

        builder.HasOne(b => b.Medicine)
               .WithMany(m => m.MedicineBatches)
               .HasForeignKey(b => b.MedicineId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
