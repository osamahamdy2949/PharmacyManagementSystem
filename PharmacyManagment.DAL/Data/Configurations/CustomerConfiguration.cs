using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.Name).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Phone).IsRequired().HasMaxLength(11);
        builder.Property(c => c.TotalSpent).HasColumnType("decimal(12,2)");
        builder.Property(c => c.TotalDebt).HasColumnType("decimal(12,2)");
        builder.Property(c => c.TotalPaid).HasColumnType("decimal(12,2)");
        builder.Property(c => c.RemainingBalance).HasColumnType("decimal(12,2)");
    }
}
