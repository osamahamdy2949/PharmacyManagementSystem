using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(s => s.Email).IsUnique();
        builder.Property(s => s.Phone).IsRequired().HasMaxLength(11);
        builder.HasIndex(s => s.Phone).IsUnique();
        builder.Property(s => s.Address).HasMaxLength(300);
        builder.Property(s => s.TaxRegNumber).HasMaxLength(50);
    }
}
