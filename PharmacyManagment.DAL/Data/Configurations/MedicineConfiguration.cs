using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.Property(m => m.SerialNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(m => m.SerialNumber).IsUnique();
        builder.Property(m => m.MedicineForm).IsRequired();
        builder.Property(m => m.PurchaseUnit).IsRequired();
        builder.Property(m => m.SaleUnit).IsRequired();
        builder.Property(m => m.UnitsPerPurchaseUnit).IsRequired();
        builder.Property(m => m.TradeName).IsRequired().HasMaxLength(50);
        builder.Property(m => m.ScientificName).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Description).HasMaxLength(200);
        builder.Property(m => m.PurchasePrice).HasColumnType("decimal(10,2)");
        builder.Property(m => m.SellingPrice).HasColumnType("decimal(10,2)");
        builder.Property(m => m.Manufacturer).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Barcode).HasMaxLength(50);
        builder.HasIndex(m => m.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
        builder.Property(m => m.StrengthValue).HasColumnType("decimal(10,2)");
        builder.Property(m => m.StrengthUnit).HasMaxLength(20);
        builder.Property(m => m.MinStockLevel).HasDefaultValue(10);
        builder.Property(m => m.IsActive).HasDefaultValue(true);
        builder.HasOne(m => m.Category).WithMany(c => c.Medicines).HasForeignKey(m => m.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

