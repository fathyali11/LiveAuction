using LiveAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LiveAuction.Infrastructure.Presistence.EntitiesConfiguration;

internal class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasIndex(x=>x.UserId);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(x=>x.User)
            .WithMany(u=>u.Notifications)
            .HasForeignKey(x=>x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}