using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.DAL.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Message).IsRequired().HasMaxLength(500);
        builder.Property(n => n.UserId).HasMaxLength(450);
        builder.Property(n => n.IsRead).HasDefaultValue(false);
    }
}
